/// Copyright (c) Michael S. Scherotter
/// spherical.js v 1.2
"use strict";

var spherical = (function () {
    "use strict";
    /// <dictionary target='function'>fov</dictionary>

    /// <dictionary target='variable'>Pos</dictionary>

    /// <dictionary>fov,Pos</dictionary>

    /// <disable>JS2085,JS3058,JS2038,JS3092,JS3057,JS3053</disable>
    /// <disable>EnableStrictMode,DeclareVariablesBeforeUse,DoNotReferenceUndefined,DeclarePropertiesBeforeUse,AvoidImplicitTypeCoercion,IncorrectNumberOfArguments</disable>
    /// <disable>JS2085.EnableStrictMode,JS3058.DeclareVariablesBeforeUse,JS2038.DoNotReferenceUndefined,JS3092.DeclarePropertiesBeforeUse,JS3057.AvoidImplicitTypeCoercion,JS3053.IncorrectNumberOfArguments</disable>

    var canvas = document.getElementById("renderCanvas"); // Get the canvas element.

    var engine = new BABYLON.Engine(canvas, true); // Generate the BABYLON 3D engine.

    var scene;
    var _camera = null;
    var _dome;
    var _zoomLevel = 1;


    /******* Add the create scene function ******/
    var createScene = function () {

        // Create the scene space
        scene = new BABYLON.Scene(engine);

        // This creates and positions a free camera (non-mesh)
        //_camera = new BABYLON.ArcRotateCamera("camera1",
        //    -Math.PI / 2,
        //    Math.PI / 2.0,
        //    5,
        //    new BABYLON.Vector3.Zero(),
        //    scene);

        //_camera.upperRadiusLimit = 180;
        //_camera.lowerRadiusLimit = 1;

        //// This attaches the camera to the canvas
        //_camera.attachControl(canvas, true);

        scene.registerAfterRender(function () {
            _dome.fovMultiplier = _zoomLevel;
        });

        scene.onPointerObservable.add(function (e) {
            if (_dome === undefined) { return; }
            _zoomLevel += e.event.wheelDelta * -0.0005;
            if (_zoomLevel < 0) { _zoomLevel = 0; }
            if (_zoomLevel > 2) { _zoomLevel = 2; }
            fovChanged();
        }, BABYLON.PointerEventTypes.POINTERWHEEL);

        var _pointerDownCount = 0;
        var _pointer1Pos = null;
        var _pointer1Id = null;
        var _pointer2Pos = null;
        var _pointer2Id = null;
        var _originalDelta = null;
        var _initialZoomLevel = _zoomLevel;

        function getDistance() {
            return Math.sqrt(
                Math.pow(_pointer2Pos.x - _pointer1Pos.x, 2) +
                Math.pow(_pointer2Pos.y - _pointer1Pos.y, 2));
        }

        scene.onPointerObservable.add(function (e) {
            if (_dome === undefined) { return; }
            switch (_pointerDownCount) {
                case 0:
                    _pointer1Pos = { x: e.event.x, y: e.event.y };
                    _pointer1Id = e.event.pointerId;
                    _pointerDownCount = 1;
                    break;
                case 1:
                    _pointer2Pos = { x: e.event.x, y: e.event.y };
                    _pointerDownCount = 2;
                    _pointer2Id = e.event.pointerId;
                    _originalDelta = getDistance();
                    _initialZoomLevel = _zoomLevel;
                    break;

                default:
                    break;
            }
        }, BABYLON.PointerEventTypes.POINTERDOWN);

        scene.onPointerObservable.add(function (e) {
            if (_dome === undefined) { return; }
            switch (_pointerDownCount) {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    if (e.event.pointerId === _pointer1Id) {
                        _pointer1Pos = { x: e.event.x, y: e.event.y };
                    } else if (e.event.pointerId === _pointer2Id) {
                        _pointer2Pos = { x: e.event.x, y: e.event.y };
                    }
                    var newDistance = getDistance();
                    var deltaPercentage = _originalDelta / newDistance;
                    _zoomLevel = Math.max(0, Math.min(2, _initialZoomLevel * deltaPercentage));
                    fovChanged();
                    break;
                default:
                    break;
            }

        }, BABYLON.PointerEventTypes.POINTERMOVE);

        scene.onPointerObservable.add(function (e) {
            if (_dome === undefined) { return; }
            switch (_pointerDownCount) {
                case 0:
                    break;
                default:
                    _pointerDownCount--;
                    break;
            }
        }, BABYLON.PointerEventTypes.POINTERUP);

        //_camera.inputs.attached.mousewheel.detachControl(canvas);

        _dome = new BABYLON.PhotoDome("drawing360", "scene.png?cache=0", {
            resolution: 32,
            size: 1000,
            useDirectMapping: false
        },
            scene);

        //setMaterial();

        var viewModeQuery = "?viewMode";

        var viewMode = "SingleView";// document.location.search.substr(viewModeQuery.length + 1);

        sphericalGui.addSphericalPanningCameraToScene(scene, canvas);

        var sphericalPanningCamera = scene.activeCamera;

        _camera = scene.activeCamera;

        var vrExperience = null;

        switch (viewMode) {
            case "None":
                break;
            case "SingleView":
                var vrOptions = {
                    createFallbackVRDeviceOrientationFreeCamera: false
                };
                vrExperience = scene.createDefaultVRExperience(vrOptions);
                sphericalGui.addUI(scene, _camera);
                break;
            case "HeadMountedDisplay":
                vrExperience = scene.createDefaultVRExperience();
                sphericalGui.addUI(scene, _camera);
                break;

            default:
                vrExperience = scene.createDefaultVRExperience();
                sphericalGui.addUI(scene, _camera);
                break;
        }

        scene.activeCamera = sphericalPanningCamera;

        if (vrExperience !== null) {
            vrExperience.onExitingVRObservable.add(() => {
                setTimeout(() => {
                    scene.activeCamera = sphericalPanningCamera;
                }, 0);
            });
        }
    };

    var cacheBuster = 1;

    function imageUpdated(base64) {
        var base64StringOrBitsArray = "data:image/jpg;base64," + base64;
        var newTexture = new BABYLON.Texture("data:scene" + cacheBuster, scene,
            false, true, BABYLON.Texture.TRILINEAR_SAMPLINGMOD, null, null, base64StringOrBitsArray, true);

        newTexture.onLoadObservable.add(function () {

            if (_dome.photoTexture !== null) {
                _dome.photoTexture.dispose();
            }

            _dome.photoTexture = newTexture;

            cacheBuster++;
        });

    }
    function setMaterial() {
        var newTexture = new BABYLON.Texture("scene.png?cache=" + cacheBuster);

        newTexture.onLoadObservable.add(function () {

            if (_dome.photoTexture !== null) {
                _dome.photoTexture.dispose();
            }

            _dome.photoTexture = newTexture;

            cacheBuster++;
        });
    }

    function getPosition() {
        return JSON.stringify({
            alpha: _camera.alpha,
            beta: _camera.beta
        });
    }

    function setPositionAB(position) {

        var ab = JSON.parse(position);

        var frameRate = 10;
        var alphaAnimation = new BABYLON.Animation("alpha", "alpha", frameRate, BABYLON.Animation.ANIMATIONTYPE_FLOAT, BABYLON.Animation.ANIMATIONLOOPMODE_CONSTANT);

        var keyFrames = [];
        keyFrames.push({ frame: 0, value: _camera.alpha });
        keyFrames.push({ frame: frameRate, value: ab.alpha });
        alphaAnimation.setKeys(keyFrames);

        var easingAlpha = new BABYLON.CubicEase();

        easingAlpha.setEasingMode(BABYLON.EasingFunction.EASINGMODE_EASEINOUT);

        alphaAnimation.setEasingFunction(easingAlpha);

        var betaAnimation = new BABYLON.Animation("beta", "beta", frameRate, BABYLON.Animation.ANIMATIONTYPE_FLOAT, BABYLON.Animation.ANIMATIONLOOPMODE_CONSTANT);

        var keyFramesB = [];
        keyFramesB.push({ frame: 0, value: _camera.beta });
        keyFramesB.push({ frame: frameRate, value: ab.beta });
        betaAnimation.setKeys(keyFramesB);

        var easingBeta = new BABYLON.CubicEase();

        easingBeta.setEasingMode(BABYLON.EasingFunction.EASINGMODE_EASEINOUT);

        betaAnimation.setEasingFunction(easingBeta);

        scene.beginDirectAnimation(_camera, [alphaAnimation, betaAnimation], 0, 2 * frameRate, false);
    }

    // set the position with yaw/pitch (alpha and beta in a json structure)
    function setPosition(position) {
        var ab = JSON.parse(position);

        setPosition2(ab.alpha, ab.beta);
    }
    // set the position with yaw/pitch
    function setPosition2(alpha, beta) {

        var roll = 0;

        var alphaOffset = -Math.PI / 2;

        var betaMultiplier = -1.0;

        var betaOffset = Math.PI / 2;

        var quaternion = BABYLON.Quaternion.RotationYawPitchRoll(-alpha + alphaOffset, (beta * betaMultiplier) + betaOffset, roll);

        // _camera.rotationQuaternion = quaternion;

        var frameRate = 10;

        var alphaAnimation = new BABYLON.Animation(
            "cameraRotation",
            "rotationQuaternion",
            frameRate,
            BABYLON.Animation.ANIMATIONTYPE_QUATERNION,
            BABYLON.Animation.ANIMATIONLOOPMODE_CONSTANT);

        var keyFrames = [];
        keyFrames.push({ frame: 0, value: _camera.rotationQuaternion });
        keyFrames.push({ frame: frameRate, value: quaternion });
        alphaAnimation.setKeys(keyFrames);

        var easingAlpha = new BABYLON.CubicEase();

        easingAlpha.setEasingMode(BABYLON.EasingFunction.EASINGMODE_EASEINOUT);

        alphaAnimation.setEasingFunction(easingAlpha);

        scene.beginDirectAnimation(_camera, [alphaAnimation], 0, 2 * frameRate, false);
    }

    function zoom(direction) {
        if (direction === "in") {
            if (_zoomLevel > 0.1) {
                _zoomLevel -= 0.1;
                fovChanged();
            }
        } else {
            if (_zoomLevel < 1.9) {
                _zoomLevel += 0.1;
                fovChanged();
            }

        }
    }

    function setZoomLevel(value) {
        if (value > 0.1 && value < 1.9) {
            _zoomLevel = value;
            fovChanged();
        }
    }

    function getZoomLevel() {
        return _zoomLevel;
    }

    createScene(); // Call the createScene function

    engine.runRenderLoop(function () { // Register a render loop to repeatedly render the scene
        scene.render();
    });

    window.addEventListener("resize", function () { // Watch for browser/canvas resize events
        engine.resize();
    });

    function fovChanged() {
        var message = JSON.stringify({
            type: "fovChanged",
            value: _zoomLevel
        });

        window.external.notify(message);
    }

    function resetPointerDownCount() {
        _pointerDownCount = 0;
    }


    return {
        zoom: zoom,
        setPosition: setPosition,
        setPosition2: setPosition2,
        getPosition: getPosition,
        setMaterial: setMaterial,
        imageUpdated: imageUpdated,
        getZoomLevel: getZoomLevel,
        setZoomLevel: setZoomLevel,
        resetPointerDownCount: resetPointerDownCount
    };
})();

function resetPointerDownCount() {
    spherical.resetPointerDownCount();
}
function setPosition(pos) {
    spherical.setPosition(pos);
}
function setPosition2(alpha, beta) {
    spherical.setPosition2(alpha, beta);
}

function zoom(direction) {
    spherical.zoom(direction);
}

function setZoomLevel(level) {
    //console.log("set zoom level:" + level);

    spherical.setZoomLevel(level);
}

function getZoomLevel() {
    return spherical.getZoomLevel();
}

function getPosition() {
    return spherical.getPosition();
}

function setMaterial() {
    spherical.setMaterial();
}

function imageUpdated(base64) {
    spherical.imageUpdated(base64);
}
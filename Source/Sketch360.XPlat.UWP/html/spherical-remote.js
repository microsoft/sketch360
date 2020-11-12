/// Copyright (c) Michael S. Scherotter
/// spherical-remote.js v 1.1
/// <dictionary target='variable'>Pos</dictionary>
/// <dictionary>Pos</dictionary>
    "use strict";

var spherical_remote = (function () {
    "use strict";
    var canvas = document.getElementById("renderCanvas"); // Get the canvas element 

    var engine = new BABYLON.Engine(canvas, true); // Generate the BABYLON 3D engine

    var scene;
    var _camera;
    var _dome;
    var _zoomLevel = 1;

    /******* Add the create scene function ******/
    var createScene = function () {

        // Create the scene space
        scene = new BABYLON.Scene(engine);

        scene.registerAfterRender(function () {
            _dome.fovMultiplier = _zoomLevel;
        });

        scene.onPointerObservable.add(function (e) {
            if (_dome === null) { return; }
            _zoomLevel += e.event.wheelDelta * -0.0005;
            if (_zoomLevel < 0) { _zoomLevel = 0; }
            if (_zoomLevel > 2) { _zoomLevel = 2; }
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
            if (_dome === null) { return; }
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
            if (_dome === null) { return; }
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
                    var deltaPercentage = newDistance / _originalDelta;
                    _zoomLevel = Math.max(0, Math.min(2, _initialZoomLevel * deltaPercentage));
                    break;
                default:
                    break;
            }

        }, BABYLON.PointerEventTypes.POINTERMOVE);

        scene.onPointerObservable.add(function (e) {
            if (_dome === null) { return; }
            switch (_pointerDownCount) {
                case 0:
                    break;
                default:
                    _pointerDownCount--;
                    break;
            }
        }, BABYLON.PointerEventTypes.POINTERUP);


        _dome = new BABYLON.PhotoDome("drawing360", "sketch.jpg", {
            resolution: 32,
            size: 1000,
            useDirectMapping: false
        },
            scene);

        var viewModeQuery = "?viewMode";

        var viewMode = document.location.search.substr(viewModeQuery.length + 1);

        sphericalGui.addSphericalPanningCameraToScene(scene, canvas);
        var sphericalPanningCamera = scene.activeCamera;
         _camera = scene.activeCamera;
        var vrExperience;

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

        _camera = scene.activeCamera;

        vrExperience.onExitingVRObservable.add(() => {
            setTimeout(() => {
                scene.activeCamera = sphericalPanningCamera;
            }, 0);
        });
    };

    var cacheBuster = 1;

    function setMaterial() {
        var newTexture = new BABYLON.Texture("scene.png?cache=" + cacheBuster);

        _dome.photoTexture = newTexture;

        cacheBuster++;
    }

    function zoom(direction) {
        if (direction === "in") {
            if (_dome.fovMultiplier > 0) {
                _dome.fovMultiplier -= 0.1;
            }
        } else {
            if (_dome.fovMultiplier < 2) {
                _dome.fovMultiplier += 0.1;
            }

        }
    }

    createScene(); // Call the createScene function

    engine.runRenderLoop(function () { // Register a render loop to repeatedly render the scene
        scene.render();
    });

    window.addEventListener("resize", function () { // Watch for browser/canvas resize events
        engine.resize();
    });
    return {
        zoom: zoom,
        setMaterial: setMaterial
    };
})();

function zoom(direction) {
    spherical_remote.zoom(direction);
}

function setMaterial() {
    spherical_remote.setMaterial();
}

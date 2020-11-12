(function () {
    "use strict";

    /// <disable>JS2085,JS3058</disable>
    /// <disable>EnableStrictMode,DeclareVariablesBeforeUse</disable>
    /// <disable>JS2085.EnableStrictMode,JS3058.DeclareVariablesBeforeUse</disable>

    var canvas = document.getElementById("renderCanvas"); // Get the canvas element 

    var engine = new BABYLON.Engine(canvas, true); // Generate the BABYLON 3D engine
    var createScene = function () {
        var scene = new BABYLON.Scene(engine);
        var camera = new BABYLON.ArcRotateCamera("Camera", -Math.PI / 2, Math.PI / 2, 5, BABYLON.Vector3.Zero(), scene);
        camera.attachControl(canvas, true);

        camera.inputs.attached.mousewheel.detachControl(canvas);

        var dome = new BABYLON.PhotoDome(
            "grid",
            "grid.jpg",
            {
                resolution: 32,
                size: 1000,
                useDirectMapping: false
            },
            scene
        );

        return scene;
    };

    var newScene = createScene(); // Call the createScene function

    engine.runRenderLoop(function () { // Register a render loop to repeatedly render the scene
        newScene.render();
    });
})();


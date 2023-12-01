function Run(){
    CopyDocument();

    var smartObjects = new Array();
    var rootLayers = app.activeDocument.artLayers; //get layers at the root level.
    if(rootLayers.length > 0) {
        FillSmartObjectsFromLayers(rootLayers, smartObjects); //get the smart obs in these layers
    }

    var rootLayerSets =  app.activeDocument.layerSets; //get root layers sets
    for(var i = 0; i < rootLayerSets.length; i++) {
        var layersInSet = rootLayerSets[i].layers;
        FillSmartObjectsFromLayers(layersInSet, smartObjects);
    }

    ConvertSmartObjects(smartObjects);

    app.activeDocument.save();
}

function CopyDocument() {
    var documentFile = new File(activeDocument.fullName);
    var copyFile = documentFile.saveDlg();
    psdSaveOptions = new PhotoshopSaveOptions();
    psdSaveOptions.embedColorProfile = true;
    psdSaveOptions.alphaChannels = true;
    psdSaveOptions.layers = true;
    psdSaveOptions.annotations = true;
    psdSaveOptions.spotColors = true;
    app.activeDocument.saveAs(copyFile, psdSaveOptions, true, Extension.LOWERCASE);
    app.open(new File(copyFile));
}

function FillSmartObjectsFromLayers(layers, smartObjects){
    try {
        if(layers.length == 0) return;
        for(var i = 0; i < layers.length; i++) {
            if(layers[i].kind != LayerKind.SMARTOBJECT) continue;
            smartObjects.push(layers[i]);
        }
    } catch(e) {
        alert(e);
    }
}

function ConvertSmartObjects(smartObjects){
    try {
        var existingFiles = new Object();
        if(smartObjects.length == 0) return;
        for(var i = 0; i < smartObjects.length; i++) {
            app.activeDocument.activeLayer = smartObjects[i];
            app.runMenuItem(stringIDToTypeID('placedLayerEditContents'));
            var smartObjectName = app.activeDocument.name;
            if (existingFiles[smartObjectName] == undefined) {
                app.activeDocument.artLayers.add();
                app.activeDocument.mergeVisibleLayers();
                existingFiles[smartObjectName] = true;
            }
            app.activeDocument.close(SaveOptions.SAVECHANGES);
        }
    } catch(e) {
        alert(e);
    }
}
Run();
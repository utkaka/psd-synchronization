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
    var linkedDocumentsFolder = new Folder(activeDocument.fullName.path + "/LinkedDocuments");
    if (!linkedDocumentsFolder.exists) linkedDocumentsFolder.create();
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
            var ref = new ActionReference();
            ref.putEnumerated( charIDToTypeID("Lyr "), charIDToTypeID("Ordn"), charIDToTypeID("Trgt") );

            app.runMenuItem(stringIDToTypeID('placedLayerEditContents'));
            var idplacedLayerUpdateAllModified = stringIDToTypeID( "placedLayerUpdateAllModified" );
            executeAction( idplacedLayerUpdateAllModified, undefined, DialogModes.NO);
            var smartObjectName = activeDocument.name;
            app.activeDocument.close(SaveOptions.SAVECHANGES);
            executeActionGet(ref);
            if (existingFiles[smartObjectName] == undefined) {
                var path = activeDocument.fullName.path + "/LinkedDocuments/" + smartObjectName;
                existingFiles[smartObjectName] = ConvertSmartObjectToLinked(path);
            } else {
                LinkSmartObjectToLinked(existingFiles[smartObjectName]);
            }
        }
    } catch(e) {
        alert(e);
    }
}

function ConvertSmartObjectToLinked(filePath) {
    var file = new File(filePath);
    var idplacedLayerConvertToLinked = stringIDToTypeID("placedLayerConvertToLinked");
    var convertAction = new ActionDescriptor();
    var ref = new ActionReference();
    ref.putEnumerated( charIDToTypeID("Lyr "), charIDToTypeID("Ordn"), charIDToTypeID("Trgt"));
    convertAction.putReference(charIDToTypeID("null"), ref );
    var idUsng = charIDToTypeID("Usng");
    convertAction.putPath(idUsng, file);
    executeAction(idplacedLayerConvertToLinked, convertAction, DialogModes.NO);
    return file;
}

function LinkSmartObjectToLinked(file) {
    var desc = new ActionDescriptor();
    desc.putPath(charIDToTypeID('null'), file);
    executeAction(stringIDToTypeID('placedLayerRelinkToFile'), desc, DialogModes.NO);
}

Run();
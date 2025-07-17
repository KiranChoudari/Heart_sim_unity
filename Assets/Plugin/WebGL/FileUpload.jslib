mergeInto(LibraryManager.library, {
  OpenFileDialogForWebGL: function (gameObjectNamePtr, methodNamePtr) {
    const gameObjectName = UTF8ToString(gameObjectNamePtr);
    const methodName = UTF8ToString(methodNamePtr);

    const input = document.createElement("input");
    input.type = "file";
    input.accept = ".json";
    input.onchange = function (event) {
      const file = event.target.files[0];
      if (!file) return;

      const reader = new FileReader();
      reader.onload = function (e) {
        const contents = e.target.result;
        // Send to Unity
        SendMessage(gameObjectName, methodName, contents);
      };
      reader.readAsText(file);
    };
    input.click();
  }
});

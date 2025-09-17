// Based on a concept by Domenico Rotolo on YouTube: https://www.youtube.com/watch?app=desktop&v=fg_aiGVeKc4&ab_channel=uNicoDev

// Add a custom JavaScript function to Unity's WebGL build by merging into the LibraryManager.
mergeInto(LibraryManager.library, {

  // Define a custom function that Unity can call: GetJSON
  // Parameters:
  // - path: the location in the Firebase Realtime Database to read from
  // - objectName: the name of the Unity GameObject that will receive the callback
  // - callback: the name of the Unity method to call if successful
  // - fallback: the name of the Unity method to call if there's an error
  GetJSON: function (path, objectName, callback, fallback) {

    // Convert the C-style strings from Unity into regular JavaScript strings.
    var parsedPath = UTF8ToString(path);
    var parsedObjectName = UTF8ToString(objectName);
    var parsedCallback = UTF8ToString(callback);
    var parsedFallback = UTF8ToString(fallback);
    
    // Use the Firebase JavaScript SDK to read data once from the specified path.
    // The .once('value') method returns a Promise, which is used for asynchronous operations.
    firebase.database().ref(parsedPath).once('value')
      .then(function(snapshot) {
        // If the database read is successful, this code block will execute.
        // The data is converted to a JSON string first, then sent back to Unity.
        window.unityInstance.SendMessage(parsedObjectName, parsedCallback, JSON.stringify(snapshot.val()));
      })
      .catch(function(error) {
        // This is the correct way to handle errors from the asynchronous operation.
        // If the read fails (e.g., network error, permission denied), this block executes.
        window.unityInstance.SendMessage(parsedObjectName, parsedFallback, "There was a Firebase error: " + error.message);
      });
  }

});
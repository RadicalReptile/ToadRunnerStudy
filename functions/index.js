// Import the Firebase Cloud Functions SDK.
const functions = require("firebase-functions");
// Import the Firebase Admin SDK (used to access the Realtime Database).
const admin = require("firebase-admin");

// Initialize the Firebase Admin SDK using the project's default credentials.
admin.initializeApp();

// List of valid group names the functions support.
// This is defined once to avoid redundancy.
const VALID_GROUPS = [
  "TestGroup1Text",
  "TestGroup2Arrows",
  "ControlGroupBlank",
];

// Saves the GUID to the Realtime Database (RTDB).
// This function only responds to HTTP request POST.
exports.registerUnityId = functions.https.onRequest(async (req, res) => {
  // Set CORS headers for all responses.
  // TODO: replace "*" with WebGL game host URL
  res.set("Access-Control-Allow-Origin", "*");
  res.set("Access-Control-Allow-Methods", "POST, OPTIONS");
  res.set("Access-Control-Allow-Headers", "Content-Type");

  if (req.method === "OPTIONS") {
    return res.status(204).send("");
  }

  // Require POST for security.
  if (req.method !== "POST") {
    return res.status(405).send("Method Not Allowed");
  }

  const {token, unityId, direction, group} = req.body;

  // Check token.
  const validToken = process.env.SURVEY_TOKEN;
  if (token !== validToken) {
    return res.status(403).send("Unauthorized: Invalid token");
  }

  if (!unityId) {
    return res.status(400).send("Missing unityId");
  }

  if (!direction || !["left", "right"].includes(direction.toLowerCase())) {
    return res.status(400).send("Invalid or missing direction");
  }

  if (!group || !VALID_GROUPS.includes(group)) {
    return res.status(400).send("Invalid or missing group");
  }

  try {
    // Only create the record if it doesn't already exist.
    const ref = admin.database().ref(`unity_ids/${unityId}`);
    const snapshot = await ref.once("value");

    // Return a 409 conflict status, if already used.
    if (snapshot.exists()) {
      return res.status(409).send("Unity ID already registered");
    }

    await ref.set({
      status: false,
      direction: direction.toLowerCase(),
      group: group,
      createdAt: admin.database.ServerValue.TIMESTAMP,
    });

    res.status(200).send("Unity ID registered successfully");
  } catch (error) {
    console.error(error);
    res.status(500).send("Error registering Unity ID");
  }
});

// Export a Cloud Function named "incrementGroup".
// This function responds to HTTP requests GET or POST.
exports.incrementGroup = functions.https.onRequest(async (req, res) => {
  // Set CORS headers for all responses.
  // TODO: replace "*" with survey host (or Apps Script or Sheets?) URL
  res.set("Access-Control-Allow-Origin", "*");
  res.set("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
  res.set("Access-Control-Allow-Headers", "Content-Type");

  // Handle preflight OPTIONS requests.
  if (req.method === "OPTIONS") {
    return res.status(204).send("");
  }

  // Note: Your Apps Script sends GET requests with query parameters.
  // We use the body for POST and query for GET.
  const token = req.query.token || req.body.token;
  const group = req.query.group || req.body.group;
  const submissionId = req.query.submissionId || req.body.submissionId;
  const direction = req.query.direction || req.body.direction;

  // Token generated via: https://it-tools.tech/token-generator
  // This is used to prevent unauthorized access to the function.
  const validToken = process.env.SURVEY_TOKEN;
  if (token !== validToken) {
    return res.status(403).send("Unauthorized: Invalid token");
  }

  // Get the new unique submission ID (Unity GUID in this case).
  if (!submissionId) {
    return res.status(400).send("Missing submissionId");
  }

  // If the provided group is not in the list, return an error.
  if (!group || !VALID_GROUPS.includes(group)) {
    return res.status(400).send("Invalid group");
  }

  // Validate the direction.
  if (!direction || !["left", "right"].includes(direction)) {
    return res.status(400).send("Invalid or missing direction");
  }

  // The function's core purpose: ----------------------------------------------
  try {
    // STEP 1: Check if the Unity ID was pre-registered and unused.
    const unityRef = admin.database().ref(`unity_ids/${submissionId}`);
    const unitySnapshot = await unityRef.once("value");

    if (!unitySnapshot.exists()) {
      return res.status(400).send("Invalid Unity ID");
    }

    const unityData = unitySnapshot.val();

    // If GUID already used, prevent duplicate survey entry.
    if (unityData.status === true) {
      return res
          .status(200)
          .send(`Unity ID ${submissionId} already processed`);
    }

    // Check that both recorded group and direction match the survey parameters.
    if (unityData.group !== group || unityData.direction !== direction) {
      return res.status(400).send("Group or direction mismatch");
    }

    // Mark Unity ID as used before proceeding.
    await unityRef.update({
      status: true,
      usedAt: admin.database.ServerValue.TIMESTAMP,
    });

    // STEP 2: Safely increment the group count in the RTDB.
    const ref = admin.database().ref(`group_counts/${group}`);
    await ref.transaction((current) => (current || 0) + 1);

    // Send back a success message if the update succeeded.
    res.status(200).send(`Incremented group: ${group}`);
  } catch (error) {
    console.error(error);
    res.status(500).send("Error incrementing group");
  }
  // ---------------------------------------------------------------------------
});

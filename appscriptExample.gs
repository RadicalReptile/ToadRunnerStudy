function onChange(e) {
  // Only continue if the change event type is EDIT or INSERT_ROW.
  // This avoids running the script for other change types (like formatting or other sheet changes).
  if (!e.changeType || (e.changeType !== 'EDIT' && e.changeType !== 'INSERT_ROW')) {
    Logger.log(`Ignored changeType: ${e.changeType}`);
    return;
  }

  const sheetName = "Toad Runner Responses";
  const spreadsheet = SpreadsheetApp.getActiveSpreadsheet();
  const sheet = spreadsheet.getSheetByName(sheetName);

  // If the sheet does not exist, stop the script and log the error.
  if (!sheet) {
    Logger.log("Sheet not found");
    return;
  }

  // Get all data in the sheet including headers and all rows.
  const dataRange = sheet.getDataRange();
  const allRows = dataRange.getValues();
  const headers = allRows[0];

  // Helper function to find the index of a given header (case-insensitive):
  function findHeaderIndex(headerName) {
    return headers.findIndex(
      h => h.toString().toLowerCase() === headerName.toLowerCase()
    );
  }

  // Find the column indices for the relevant headers.
  const groupColIndex = findHeaderIndex("group");
  const submissionIdColIndex = findHeaderIndex("unity id");
  const processedColIndex = findHeaderIndex("processed");
  const directionColIndex = findHeaderIndex("direction"); // <-- New column for x/y

  // If any required column is missing, log and exit.
  if (groupColIndex === -1 || submissionIdColIndex === -1 || processedColIndex === -1 || directionColIndex === -1) {
    Logger.log("Missing one or more required columns: Group, Unity ID, Processed, or Direction");
    return;
  }

  // Loop through each row starting after the header row: -----------------------------------------------------------
  for (let i = 1; i < allRows.length; i++) {
    const row = allRows[i];

    // Skip if the submission is already marked as processed ("Yes").
    if (row[processedColIndex] && row[processedColIndex].toString().toLowerCase() === "yes") {
      continue;
    }

    // Extract submission ID, group code, and direction code from the current row.
    const submissionId = row[submissionIdColIndex];
    const groupCode = row[groupColIndex];
    const directionCode = row[directionColIndex];

    // If missing submission ID, group code, or direction code, mark as error and skip.
    if (!submissionId || !groupCode || !directionCode) {
      sheet.getRange(i + 1, processedColIndex + 1).setValue("Missing submissionId, groupCode, or direction");
      continue;
    }

    // Map single-letter group codes (a, b, c) to full Firebase group names:
    const groupMap = {
      a: "TestGroup1Text",
      b: "TestGroup2Arrows",
      c: "ControlGroupBlank"
    };
    const group = groupMap[groupCode.toLowerCase()];

    // If the group code is invalid, mark as error and skip.
    if (!group) {
      sheet.getRange(i + 1, processedColIndex + 1).setValue("Invalid group code");
      continue;
    }

    // Map x/y to descriptive direction values for Firebase (optional, could just send x/y directly)
    const directionMap = {
      x: "left",
      y: "right"
    };
    const direction = directionMap[directionCode.toLowerCase()];

    // If the direction code is invalid, mark as error and skip.
    if (!direction) {
      sheet.getRange(i + 1, processedColIndex + 1).setValue("Invalid direction code");
      continue;
    }

    const token = PropertiesService.getScriptProperties().getProperty('SURVEY_TOKEN');
    const baseUrl = 'https://us-central1-toadrunnerstudy.cloudfunctions.net/incrementGroup';

    // Construct the full URL with query parameters, encoding each value for safety:
    // GET is fine security-wise because it’s backend-to-backend with secret tokens.
    const url = `${baseUrl}?token=${encodeURIComponent(token)}`
              + `&group=${encodeURIComponent(group)}`
              + `&submissionId=${encodeURIComponent(submissionId)}`
              + `&direction=${encodeURIComponent(direction)}`;

    // Call the Firebase function: -----------------------------------------------------------
    try {
      // Make a GET request to your Firebase function.
      const response = UrlFetchApp.fetch(url, { method: 'get', muteHttpExceptions: true });
      const responseText = response.getContentText();

      // If status 200, mark the row as processed with "Yes".
      if (response.getResponseCode() === 200) {
        sheet.getRange(i + 1, processedColIndex + 1).setValue("Yes");
        Logger.log(`Processed submission ${submissionId}: ${responseText}`);
      } else {
        // Otherwise, log the error response on the sheet and in logs.
        sheet.getRange(i + 1, processedColIndex + 1).setValue(`Error: ${responseText}`);
        Logger.log(`Error for submission ${submissionId}: ${responseText}`);
      }
    } catch (err) {
      // Catch network or unexpected errors, log on sheet and in logs.
      sheet.getRange(i + 1, processedColIndex + 1).setValue(`Exception: ${err.message}`);
      Logger.log(`Exception for submission ${submissionId}: ${err.message}`);
    }
    // ---------------------------------------------------------------------------------------
  }
  // ----------------------------------------------------------------------------------------------------------------
}

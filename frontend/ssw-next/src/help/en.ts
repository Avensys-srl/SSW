import type { HelpContent } from "./types";

export const enHelp: HelpContent = {
  language: "en",
  title: "SSW guided selection",
  introduction:
    "Complete the nine steps in order. You can return to a completed step without losing the current selection.",
  workflowNote:
    "Save the editable selection before generating the registered technical report. Warnings must be reviewed; invalid conditions must be corrected.",
  topics: {
    project: { title: "1. Project", summary: "Enter the project, customer and selection references used to identify the work.", tip: "Use one project for related units and a distinct reference for each selection." },
    preselection: { title: "2. Preselection", summary: "Enter airflow and pressure to obtain a first list of compatible ventilation units.", tip: "Use the real design duty point; safety margins can be checked after selecting the unit." },
    unit: { title: "3. Unit", summary: "Compare the proposed models and confirm the unit that will receive the detailed calculation.", tip: "Check pressure margin, efficiency and electrical demand together." },
    installation: { title: "4. Installation", summary: "Choose mounting position and airflow configuration, then review the dimensional layout.", tip: "Confirm access-panel orientation and duct connections before issuing the report." },
    "water-coil": { title: "5. Water coils", summary: "Select cooling, heating or combined operation and configure the compatible water coil.", tip: "Review water pressure drop and maximum outlet-air conditions." },
    "electric-heaters": { title: "6. Electric heaters", summary: "Select compatible preheating and post-heating devices when required.", tip: "PEHD changes the heat-exchanger inlet condition; EHD heats the supply air downstream." },
    accessories: { title: "7. Accessories", summary: "Choose controls, sensors, communication modules and mechanical accessories.", tip: "Standard items remain selected; disabled options show their dependency." },
    documents: { title: "8. Documents", summary: "Open the available product sheet and installation or maintenance documents.", tip: "Document availability follows the selected model and language." },
    summary: { title: "9. Summary", summary: "Review duty point, configuration, options and validation status before output.", tip: "Save the .sswsel file for later editing, then generate the traceable PDF report." },
  },
};

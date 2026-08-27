import { readdirSync, readFileSync, statSync } from "node:fs";
import { join, relative } from "node:path";

const root = process.cwd();
const expectedDefinitions = new Map([
  ["Assets/BlockPuzzleGameToolkit/Scripts", "mmgb.BlastKingdom.Main.asmdef"],
  ["Assets/BlockPuzzleGameToolkit/Scripts/Editor", "mmgb.BlastKingdom.Drawers.asmdef"],
  ["Assets/BlockPuzzleGameToolkit/Scripts/LevelsData/Editor", "mmgb.BlastKingdom.LevelEditor.asmdef"],
  ["Assets/BlockPuzzleGameToolkit/Scripts/Localization/Editor", "mmgb.Localization.Editor.asmdef"],
  ["Assets/BlockPuzzleGameToolkit/Scripts/Services/Ads", "mmgb.Ads.asmdef"],
  ["Assets/BlockPuzzleGameToolkit/Scripts/Services/Ads/Networks/LevelPlay", "mmgb.LevelPlay.asmdef"],
  ["Assets/BlockPuzzleGameToolkit/Scripts/Services/IAP", "mmgb.IAP.asmdef"],
]);

const obsoleteAssemblyNames = [
  "CandySmith.BlockPuzzle.Main",
  "CandySmith.BlockPuzzle.Drawers",
  "CandySmith.BlockPuzzle.LevelEditor",
  "CandySmith.Localization.Editor",
  "CandySmith.Ads",
  "CandySmith.LevelPlay",
  "CandySmith.IAP",
];

function findFiles(directory, predicate, output = []) {
  for (const entry of readdirSync(directory)) {
    const absolutePath = join(directory, entry);
    const stat = statSync(absolutePath);
    if (stat.isDirectory()) findFiles(absolutePath, predicate, output);
    if (stat.isFile() && predicate(absolutePath)) output.push(absolutePath);
  }
  return output;
}

const errors = [];
for (const [directory, expectedFile] of expectedDefinitions) {
  const absoluteDirectory = join(root, directory);
  const definitions = readdirSync(absoluteDirectory).filter(file => file.endsWith(".asmdef"));
  if (definitions.length !== 1 || definitions[0] !== expectedFile) {
    errors.push(`${directory} must contain only ${expectedFile}; found ${definitions.join(", ") || "none"}`);
    continue;
  }

  const metadata = readFileSync(join(absoluteDirectory, `${expectedFile}.meta`), "utf8");
  if (!metadata.includes(`assetPath: ${directory}/${expectedFile}`)) {
    errors.push(`${directory}/${expectedFile}.meta does not declare its current asset path`);
  }
}

for (const file of findFiles(join(root, "Assets"), path => path.endsWith(".asmdef") || path.endsWith(".asmref"))) {
  const content = readFileSync(file, "utf8");
  const obsoleteName = obsoleteAssemblyNames.find(name => content.includes(name));
  if (obsoleteName) errors.push(`${relative(root, file)} still references ${obsoleteName}`);
}

if (errors.length) {
  console.error("Assembly definition verification failed:");
  errors.forEach(error => console.error(`- ${error}`));
  process.exit(1);
}

console.log(`Assembly definition verification passed for ${expectedDefinitions.size} repaired folders.`);

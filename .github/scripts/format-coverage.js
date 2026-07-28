// Builds the PR / job-summary markdown from the artifacts left behind by
// `dotnet test`. Single source of truth for both feature-ci.yml and
// pr-security.yml — keeping it in one place is what stops the two workflows
// from drifting apart.
//
// Reads, relative to the current working directory (QuizzArena.Backend):
//   coverage/raw/**/*.trx          — one per test project, for the test counts
//   coverage/report/SummaryGithub.md — ReportGenerator output, for coverage
//
// Overwrites coverage/report/SummaryGithub.md with the formatted result.

const fs = require('fs');
const path = require('path');

const RAW_DIR = 'coverage/raw';
const SUMMARY_PATH = 'coverage/report/SummaryGithub.md';

// ── Test results ────────────────────────────────────────────────────────────
// Each test project emits its own .trx. The trx logger de-duplicates colliding
// names with a "[1]" suffix, so a recursive scan is enough to find them all.
function readTestCounts() {
  let files = [];
  try {
    files = fs
      .readdirSync(RAW_DIR, { recursive: true })
      .filter((f) => f.endsWith('.trx'))
      .map((f) => path.join(RAW_DIR, f));
  } catch {
    return null;
  }

  if (files.length === 0) return null;

  const totals = { total: 0, passed: 0, failed: 0, skipped: 0 };

  for (const file of files) {
    const counters = fs.readFileSync(file, 'utf8').match(/<Counters\b[^>]*\/>/);
    if (!counters) continue;

    const attr = (name) => {
      const found = counters[0].match(new RegExp(`\\b${name}="(\\d+)"`));
      return found ? Number.parseInt(found[1], 10) : 0;
    };

    totals.total += attr('total');
    totals.passed += attr('passed');
    // `error` counts tests that blew up outside the assertion path; without it
    // a crashed test would silently vanish from the summary.
    totals.failed += attr('failed') + attr('error');
    totals.skipped += attr('notExecuted');
  }

  return totals;
}

function renderTestLine(counts) {
  if (!counts) {
    return '- **Test Results:** ⚠️ no test results found (no .trx produced)';
  }

  const { total, passed, failed, skipped } = counts;
  const pct = total > 0 ? ((passed / total) * 100).toFixed(1) : '0.0';

  const parts = [`${passed} passed`];
  if (failed > 0) parts.push(`${failed} failed`);
  if (skipped > 0) parts.push(`${skipped} skipped`);

  const icon = failed > 0 ? '❌' : '✅';
  return `- **Test Results:** ${icon} ${parts.join(', ')} of ${total} (${pct}% success)`;
}

// ── Coverage ────────────────────────────────────────────────────────────────
function readCoverage(content, label) {
  const match = content.match(
    new RegExp(`${label} coverage:\\*\\* \\| (\\d+(?:\\.\\d+)?)% \\((\\d+) of (\\d+)\\)`)
  );
  return {
    pct: match ? match[1] : '0',
    covered: match ? match[2] : '0',
    total: match ? match[3] : '0',
  };
}

// ── Assemble ────────────────────────────────────────────────────────────────
const content = fs.readFileSync(SUMMARY_PATH, 'utf8');
const lines = readCoverage(content, 'Line');
const branches = readCoverage(content, 'Branch');

let prettyMd =
  '## 🧪 .NET Core Test Report\n\n' +
  '### Summary\n' +
  renderTestLine(readTestCounts()) +
  '\n\n' +
  '### 🏢 Backend Code Coverage Summary\n\n' +
  '| Metric | Coverage % | Total Lines/Branches | Covered Lines/Branches |\n' +
  '| :--- | :---: | :---: | :---: |\n' +
  `| **Lines** | ${lines.pct}% | ${lines.total} | ${lines.covered} |\n` +
  `| **Branches** | ${branches.pct}% | ${branches.total} | ${branches.covered} |\n\n`;

const coverageIndex = content.indexOf('## Coverage');
if (coverageIndex !== -1) {
  prettyMd +=
    '<details><summary>🔍 Detailed Class Breakdown</summary>\n\n' +
    content.substring(coverageIndex) +
    '\n</details>';
}

fs.writeFileSync(SUMMARY_PATH, prettyMd);

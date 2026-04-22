import fs from 'node:fs';

/**
 * Removes the SQLite file created for this Playwright run (and WAL sidecars).
 */
export default async function globalTeardown(): Promise<void> {
  const dbPath = process.env.WEIGHTLIFTING_E2E_SQLITE_FILE;
  if (!dbPath) {
    return;
  }

  for (const path of [dbPath, `${dbPath}-wal`, `${dbPath}-shm`]) {
    try {
      fs.unlinkSync(path);
    } catch {
      // File may be locked or already removed.
    }
  }
}

import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    environment: 'jsdom',
    setupFiles: ['./tests/setup.js'],
    include: ['./tests/**/*.test.js'],
    clearMocks: true,
    restoreMocks: true,
    unstubGlobals: true,
    reporters: ['default', 'json'],
    outputFile: {
      json: process.env.VITEST_RESULT_PATH ?? './TestResults/resultado-interface-javascript.json'
    },
    testTimeout: 10_000,
    hookTimeout: 10_000,
    sequence: {
      concurrent: false
    }
  }
});

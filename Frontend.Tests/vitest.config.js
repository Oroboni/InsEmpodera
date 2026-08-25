import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    environment: 'jsdom',
    setupFiles: ['./tests/setup.js'],
    include: ['./tests/**/*.test.js'],
    clearMocks: true,
    restoreMocks: true,
    unstubGlobals: true,
    testTimeout: 10_000,
    hookTimeout: 10_000,
    sequence: {
      concurrent: false
    }
  }
});

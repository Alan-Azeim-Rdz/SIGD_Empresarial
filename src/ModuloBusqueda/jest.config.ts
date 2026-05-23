import type { Config } from 'jest';

const config: Config = {
  preset:          'ts-jest',
  testEnvironment: 'node',

  // ts-jest necesita CommonJS; el tsconfig principal usa NodeNext (para producción).
  // Aquí lo sobreescribimos solo para los tests.
  transform: {
    '^.+\\.tsx?$': ['ts-jest', {
      tsconfig: {
        module:                   'CommonJS',
        moduleResolution:         'Node',
        esModuleInterop:          true,
        allowSyntheticDefaultImports: true,
        strict:                   true,
        skipLibCheck:             true,
      }
    }]
  },

  // Env vars que se inyectan ANTES de cargar cualquier módulo del test.
  // NODE_ENV=production evita que pino cree el worker de pino-pretty.
  // LOG_LEVEL=silent silencia todos los logs durante los tests.
  setupFiles: ['<rootDir>/__tests__/setup.ts'],

  testMatch:   ['**/__tests__/**/*.test.ts'],

  collectCoverage:     true,
  coverageDirectory:   'coverage',
  collectCoverageFrom: [
    'index.ts',
  ],
  coverageThreshold: {
    global: {
      branches:   80,
      functions:  80,
      lines:      80,
      statements: 80
    }
  },

  moduleFileExtensions: ['ts', 'tsx', 'js', 'jsx', 'json', 'node'],
  clearMocks:           true,
};

export default config;

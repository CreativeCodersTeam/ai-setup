---
description: 'Guidelines for building applications with Typescript'
applyTo: '**/*.ts'
---

# TypeScript Specific Instructions

**Scope: TypeScript Projects**

## Code Style
- Enable strict mode in tsconfig.json
- Use explicit types for function parameters and return values
- Avoid using 'any' type unless absolutely necessary
- Use interfaces for object shapes and types for unions/intersections

## Best Practices
- Prefer const over let, never use var
- Use arrow functions for callbacks
- Use async/await instead of raw promises
- Leverage TypeScript utility types (Partial, Pick, Omit, etc.)

## Error Handling
- Use custom error classes for specific error types
- Handle promise rejections properly
- Use type guards for narrowing types

## Testing
- Use Jest or Vitest for testing
- Write type-safe tests
- Mock external dependencies properly

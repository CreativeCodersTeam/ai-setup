---
description: 'Guidelines for Angular applications'
applyTo: '**/*.ts,**/*.html,**/*.scss'
---

# Angular Specific Instructions

**Scope: Angular Projects**

## Code Style
- Follow Angular Style Guide
- Use one component per file
- Use kebab-case for file names
- Prefix selector names with project-specific prefix

## Component Best Practices
- Use OnPush change detection strategy where possible
- Unsubscribe from observables in ngOnDestroy
- Use async pipe in templates to handle subscriptions
- Keep components focused and delegate logic to services

## Service Best Practices
- Provide services at the appropriate level (root, module, or component)
- Use dependency injection
- Keep services stateless when possible
- Use RxJS operators for complex async operations

## RxJS & Observables
Use the `rxjs` skill for observable lifecycle management and best practices.

## State Management
- Use signals for reactive state management
- Consider NgRx for complex state management needs
- Keep state immutable

## Testing
- Write unit tests for components, services, and pipes
- Use TestBed for component testing
- Mock dependencies appropriately
- Test both success and error scenarios

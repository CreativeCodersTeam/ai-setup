---
description: 'Guidelines for Java applications'
applyTo: '**/*.java'
---

# Java Specific Instructions

**Scope: Java Projects**

## Code Style
- Follow Java Code Conventions
- Use camelCase for method names and variables
- Use PascalCase for class names
- Use UPPER_SNAKE_CASE for constants

## Best Practices
- Use interfaces for abstraction
- Prefer composition over inheritance
- Use generics for type safety
- Leverage Java streams for collection operations
- Use Optional to avoid null pointer exceptions

## Framework Specifics
- Use Spring Boot for enterprise applications
- Use dependency injection with @Autowired or constructor injection
- Follow RESTful principles for API design
- Use Spring Data JPA for database access
- Use the `java-springboot` skill for Spring Boot best practices.

## Error Handling
- Use checked exceptions for recoverable conditions
- Use unchecked exceptions for programming errors
- Provide meaningful exception messages
- Use try-with-resources for auto-closeable resources

## Testing
- Use JUnit 5 for unit testing
- Use Mockito for mocking
- Write integration tests with Spring Boot Test
- Follow Given-When-Then pattern for test naming

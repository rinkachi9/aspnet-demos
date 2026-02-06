# Vertical Slice Architecture (VSA) Concept

## Introduction
Vertical Slice Architecture organizes code by **features** rather than technical layers. Instead of `Controllers`, `Services`, `Repositories` folders, you have `Features/Orders/CreateOrder`, `Features/Users/GetUser`.

### Why VSA?
- **High Cohesion**: Everything related to a feature is in one place.
- **Low Coupling**: Changing one feature doesn't break unrelated features.
- **Flexibility**: each slice can enable different patterns (Minimal API, Controller, CQRS) depending on complexity.

## The Challenge: Shared Logic
"Pure" VSA can lead to duplication. The article "The Guardrails vs. The Open Road" suggests a structured approach to sharing:

## 1. Tiers of Sharing

### Tier 1: Technical Infrastructure (Global)
- **Where**: `Infrastructure/`, `Shared/` (Kernel)
- **What**: Global concerns like Logging, Auth, DbContext, Result Pattern, Validation Pipeline.
- **Rule**: Share freely if it applies to ALL slices.

### Tier 2: Domain Concepts (Global Domain)
- **Where**: `Domain/Entities`, `Domain/ValueObjects`
- **What**: Business rules intrinsic to the entity (e.g. `Order.CanBeCancelled()`).
- **Rule**: Push logic DOWN to entities. Slices share the Domain Model.

### Tier 3: Feature-Specific Logic (Local Sharing)
- **Where**: `Features/[FeatureName]/Shared`
- **What**: logic used *only* by slices within the same feature (e.g. `OrderValidator` used by Create and Update Order).
- **Rule**: Keep it local. If you delete the Feature, this code should die with it.

### Tier 4: Cross-Feature Logic
- **Where**: `Domain/Services`
- **What**: Logic needed by multiple unrelated features (e.g. `TaxCalculator` used by `CreateOrder` and `GenerateInvoice`).
- **Rule**: If it's business logic, put it in Domain Services. If it's side-effects (e.g. Send Email), consider **Events/Messaging**.

## 3 Questions before Sharing
1. **Is it Infrastructure or Domain?** (Infra -> Share).
2. **How stable is it?** (Stable -> Share, Volatile -> Copy).
3. **Rule of Three?** (Don't abstract until you have 3 duplicates).

## Directory Structure Example
```
📂 src
└──📂 Features
│   ├──📂 Users
│   │   ├──📂 CreateUser
│   │   ├──📂 UpdateUser
│   │   └──📂 Shared          <-- Tier 3: Local Sharing
└──📂 Domain
│   ├──📂 Entities            <-- Tier 2: Shared Domain Model
│   └──📂 Services            <-- Tier 4: Cross-Feature Logic
└──📂 Infrastructure          <-- Tier 1: Technical Boilerplate
```

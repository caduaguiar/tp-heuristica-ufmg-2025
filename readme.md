# BPP-GP Solver: Bin Packing Problem with General Precedence Constraints

A GRASP (Greedy Randomized Adaptive Search Procedure) metaheuristic implementation for solving the Bin Packing Problem with General Precedence Constraints (BPP-GP).

## Problem Description

The BPP-GP is an extension of the classical Bin Packing Problem that includes precedence constraints between items. Given:
- A set of items N = {1, 2, ..., n} with weights wⱼ ∈ ℝ⁺
- A set of bins M = {1, 2, ..., m} with capacity C ∈ ℝ⁺
- A set of precedence constraints P with tuples (a, b, t_ab)

The goal is to minimize the number of bins used while:
1. Respecting bin capacities
2. Satisfying precedence constraints: i_a - i_b ≥ t_ab

## Solution Architecture

### Core Components

1. **Domain Models** (`BppGrasp.Core/Models`)
   - `Item`: Represents an item with ID and weight
   - `Bin`: Represents a bin with capacity and items
   - `PrecedenceConstraint`: Represents precedence relationships
   - `BppInstance`: Problem instance definition
   - `Solution`: Solution representation

2. **Algorithms** (`BppGrasp.Core/Algorithms`)
   - `PreProcessing`: Removes redundant precedences and sorts items
   - `BoundsCalculator`: Computes lower bounds (L0, L1, L2)
   - `GraspAlgorithm`: Main GRASP metaheuristic implementation

3. **Constructive Heuristics** (`BppGrasp.Core/Algorithms/ConstructiveHeuristics`)
   - `FirstFitDecreasingHeuristic`: FFD with randomization
   - `BestFitDecreasingHeuristic`: BFD with randomization

4. **Local Search** (`BppGrasp.Core/Algorithms/LocalSearch`)
   - `BatchingMoveLocalSearch`: Iterated local search with move and swap operations
   - `DestroyShrinkRepair`: Diversification mechanism

## Building and Running

### Prerequisites
- .NET 8.0 SDK or later

### Build
```bash
dotnet build BppGrasp.sln
```

### Run
```bash
# Run with default examples
dotnet run --project src/BppGrasp.Console

# Run with instance file
dotnet run --project src/BppGrasp.Console instances/example1.txt
```

### Run Tests
```bash
dotnet test tests/BppGrasp.Tests/BppGrasp.Tests.csproj
```

## Instance File Format

The instance file defines the complete problem specification with items, bin capacity, and precedence constraints.

### File Structure

```
<number_of_items> <bin_capacity>
<weight_item_0>
<weight_item_1>
<weight_item_2>
...
<weight_item_n-1>
<number_of_precedences>
<item_a> <item_b> <min_separation>
<item_a> <item_b> <min_separation>
...
```

### Field Descriptions

**Line 1**: Problem dimensions
- `<number_of_items>`: Total number of items to pack (integer)
- `<bin_capacity>`: Maximum capacity of each bin (real number)

**Lines 2 to n+1**: Item weights
- `<weight_item_i>`: Weight of item with ID i (real number, 0 ≤ i < n)
- Items are automatically assigned IDs from 0 to n-1 based on their order

**Line n+2**: Number of precedence constraints
- `<number_of_precedences>`: Total count of precedence relationships (integer)

**Lines n+3 onwards**: Precedence constraints
- `<item_a>`: ID of item that must come AFTER (integer)
- `<item_b>`: ID of item that must come BEFORE (integer)
- `<min_separation>`: Minimum bin separation required (integer)

### Precedence Constraint Rules

The constraint `(a, b, t)` enforces: **bin_index(a) - bin_index(b) ≥ t**

- **t = 0**: Item `a` cannot be in a bin with lower index than item `b` (can be same bin)
- **t = 1**: Item `a` must be in a strictly later bin than item `b`
- **t > 1**: At least `t` bins must separate items `b` and `a`

### Example Instance 1: Simple Case (example1.txt)

This example demonstrates basic precedence constraints with 6 items.

**File Content:**
```
6 100          ← 6 items, bin capacity = 100
50             ← Item 0: weight = 50
40             ← Item 1: weight = 40
35             ← Item 2: weight = 35
30             ← Item 3: weight = 30
25             ← Item 4: weight = 25
20             ← Item 5: weight = 20
2              ← 2 precedence constraints
0 1 1          ← Item 0 must be at least 1 bin after Item 1
2 3 0          ← Item 2 cannot be before Item 3 (can be same bin)
```

**Problem Interpretation:**
- Total weight: 200 (requires minimum 2 bins by capacity)
- Constraint 1: Item 0 (50) must come AFTER Item 1 (40) in different bins
- Constraint 2: Item 2 (35) must come AFTER or WITH Item 3 (30)

**Optimal Solution:** 3 bins
- Bin 0: Items {5, 3, 2} → weights {20, 30, 35} = 85
- Bin 1: Items {4, 1} → weights {25, 40} = 65
- Bin 2: Items {0} → weight {50} = 50

### Example Instance 2: Medium Complexity (example2.txt)

This example shows a more complex scenario with 10 items and multiple precedence chains.

**File Content:**
```
10 150         ← 10 items, bin capacity = 150
45.5           ← Item 0: weight = 45.5
62.3           ← Item 1: weight = 62.3
38.7           ← Item 2: weight = 38.7
51.2           ← Item 3: weight = 51.2
29.8           ← Item 4: weight = 29.8
41.6           ← Item 5: weight = 41.6
55.9           ← Item 6: weight = 55.9
33.4           ← Item 7: weight = 33.4
48.1           ← Item 8: weight = 48.1
36.5           ← Item 9: weight = 36.5
5              ← 5 precedence constraints
0 1 1          ← Item 0 must be 1+ bins after Item 1
1 2 0          ← Item 1 cannot be before Item 2
3 4 1          ← Item 3 must be 1+ bins after Item 4
5 6 0          ← Item 5 cannot be before Item 6
7 8 1          ← Item 7 must be 1+ bins after Item 8
```

**Problem Interpretation:**
- Total weight: 442.9 (requires minimum 3 bins by capacity)
- Creates precedence chains: {2→1→0}, {4→3}, {6→5}, {8→7}
- Some items (9) have no precedence constraints

**Optimal Solution:** 4 bins with 73.8% average utilization

### Creating Your Own Instances

**Valid Instance Requirements:**
1. All item weights must be ≤ bin capacity
2. Precedence constraints must not create cycles
3. Item IDs in constraints must be valid (0 to n-1)
4. Constraints should be satisfiable (check for conflicts)

**Testing Your Instance:**
```bash
# Create myinstance.txt following the format above
dotnet run --project src/BppGrasp.Console instances/myinstance.txt

# Use verbose mode to see detailed algorithm progress
dotnet run --project src/BppGrasp.Console instances/myinstance.txt --verbose
```

## GRASP Parameters

- **Alpha (α)**: Controls randomization in construction phase (0.0 = greedy, 1.0 = random)
- **Adaptive Alpha**: Enables dynamic α adjustment during iterations
- **Alpha Range**: [AlphaMin, AlphaMax] for adaptive mode
- **Max Time**: Maximum execution time
- **Max Iterations**: Maximum number of GRASP iterations

## Algorithm Features

### Construction Phase
- Randomized greedy construction with adaptive α
- Multiple constructive heuristics (FFD, BFD)
- Precedence-aware item placement

### Local Search Phase
- Batching Move Local Search (BM-LS)
- Item move and swap operations
- Precedence constraint validation

### Diversification
- Destroy-Shrink-Repair mechanism
- Adaptive parameter adjustment

## Performance Metrics

The solver reports:
- Number of bins used
- Gap to lower bound
- Solution feasibility
- Bin utilization statistics
- Convergence information

## Project Structure

```
├── src/
│   ├── BppGrasp.Core/          # Core library
│   │   ├── Models/             # Domain models
│   │   ├── Algorithms/         # Core algorithms
│   │   ├── IO/                 # Instance I/O
│   │   └── Utils/              # Utilities
│   └── BppGrasp.Console/       # Console application
├── tests/
│   └── BppGrasp.Tests/         # Unit tests
├── instances/                  # Example instances
└── BppGrasp.sln                # Solution file
```

## References

Based on the methodology from:
- Kramer et al. (2017) - "A batching-move iterated local search algorithm for the bin packing problem with generalized precedence constraints"
- Layeb & Chenche (2012) - "A novel GRASP algorithm for solving the bin packing problem"

## Authors

- Carlos E. Aguiar
- Pedro Saccomani
- Rangel A. Barroso

UFMG - Departamento de Ciência da Computação

## License

Academic project - UFMG 2025

# Grid for your A* implementation

The StealthSandbox scene has a **Navigation Grid** object with
`NavigationGrid2D` and your `Astar` component. The grid scans on Awake,
before normal scripts. Your Astar can access it through its `Grid` property.
Use that property in Start or later, after its Awake has run.

Select Navigation Grid and enable Gizmos. Green cells are available;
red cells overlap a wall. In edit mode use the grid component's context
menu **Generate / Rescan Grid** to preview cells. Rescan after editing
walls; the runtime scan happens automatically on entering Play Mode.

The initial grid covers the floor from (-12, -9) with 48 by 36 cells of
0.5 world units. Origin is the lower-left edge; nodes are at cell centres.
Agent Radius is 0.4, based on the guard's 0.38-radius circle plus clearance.
Walls must be on the Walls layer. Player and guard are not blocked cells.

## Access from Astar

- `Grid.Nodes`: the full `Node[,]` array.
- `Grid.GetNode(coordinate)`: look up a Vector2Int grid address.
- `Grid.GetNodeFromWorldPosition(position)`: map a world position to a node.
  Outside the grid returns null. A blocked cell returns a node whose
  `Available` is false; it does not silently choose another destination.
- `Grid.GetNeighbours(node)`: available cardinal neighbours. It also checks
  clearance along the connection so thin walls cannot be crossed.
- `Grid.ResetSearchData()`: reset costs and parent references before a search.

Node preserves your `Available`, `gridCoordinate`, and `worldPosition`
fields. It adds `gCost`, `hCost`, computed `fCost`, and `parent`.
The starting gCost is infinity; your algorithm must set its start node to 0.
Choose matching units for movement cost and heuristic in your implementation.

These search fields belong to the shared nodes. Complete one search before
resetting for the next. For simultaneous searches, store costs and parents
per search instead. Rescanning replaces nodes and invalidates old paths.

Your existing open/closed lists and the enemy movement are unchanged.
There is no A* search, heuristic, path reconstruction, or route following yet.

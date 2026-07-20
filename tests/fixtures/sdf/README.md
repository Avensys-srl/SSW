# Historical SDF fixtures

These binary fixtures are intentionally versioned because database compatibility
is part of the SSW release contract.

| File | Expected state | SHA-256 |
| --- | --- | --- |
| `legacy-0.sdf` | no metadata, accepted as `Legacy-0` | `DEF29BD798BB265D41E22F0643E1CFCBBC0277381776681B556459A6B4CB5929` |
| `schema-1-av.sdf` | managed schema 1, AV customer | `0CABF3A8D428E26B2954F4B9C8546657A81251B8FCADD979D48C8A65C9B2595C` |
| `schema-2-av.sdf` | managed schema 2, water coils and electric heaters | `023F4277C3A48C6C897F9C11D88E90954F62BB0DF9DA8106BCE3888406BFB316` |
| `schema-3-av.sdf` | managed schema 3, accessories and control functions | `D59BEF3521ABA7CDD52B15999BF94179931846D7274F1F0893948E46E8E07728` |

Never replace a historical fixture. Add a new file when a supported schema is
introduced, then extend `SelectionIdentitySmoke` with its expected features.

`schema-3-av.sdf` is derived from `schema-2-av.sdf`: its metadata schema is set
to 3, its minimum SSW version to `1.3.0.52`, and the enabled feature
`AccessoriesAndControlFunctions` version 1 is added. No application catalog
rows are required by this compatibility fixture.

# Test Research — refine-nearby-food-search

## Scope

Broad, bounded regression work across the existing Vue composable/view tests and ASP.NET Core endpoint/provider tests. Spectra remains the progress authority; this folder records only the test-generation evidence required by the testing workflow.

## Target inventory

| Production seam | Existing test file | Existing convention |
| --- | --- | --- |
| `src/EatThis.Web/src/composables/useNearbyFood.ts` | `src/EatThis.Web/src/composables/useNearbyFood.spec.ts` | Vitest, injected geolocation and `pick`, direct ref assertions |
| `src/EatThis.Web/src/App.vue` | `src/EatThis.Web/src/app.spec.ts` | Vitest + Vue Test Utils, injected props, DOM/accessibility assertions |
| `src/EatThis.Api/Domain/NearbySearchQuery.cs` and endpoint boundary | `tests/EatThis.Api.Tests/NearbyFoodEndpointTests.cs` | MSTest integration tests with provider spy/fake |
| `src/EatThis.Api/Infrastructure/GooglePlacesProvider.cs` | `tests/EatThis.Api.Tests/GooglePlacesProviderTests.cs` | MSTest with captured upstream HTTP request/response |

The required polyglot static pairing analyzer could not run because `tree-sitter-language-pack` is not installed. No global dependency was installed for this change. The explicit Spectra task paths and the existing imports/references above are used as the bounded target inventory; this is not line or branch coverage evidence.

## Requirement checklist

- Default pending radius is 100 metres and the first explicit search submits 100 after one location request.
- Changing the pending radius to 700 has no geolocation or API side effect; an explicit search submits 700.
- The maximum pending radius submits 3000; 5000-metre retry APIs and UI do not exist.
- Frontend invalid pending values below 100 or above 3000 do not submit and produce a bounded-input error naming 100 metres through 3 kilometres.
- The range control exposes min 100, max 3000, and step 100, with correct 100/700/3000 display text and busy disabling.
- No-results preserves the selected radius, names the searched range, and requires an explicit new search without automatic expansion.
- One result renders normalized Traditional Chinese text unchanged; a long Latin fallback remains visible and structurally wrappable.
- A Google-backed result has an accessible `Google Maps` attribution and never displays a standalone raw `GOOGLE` label.
- API accepts 100 and 3000, rejects 99/3001/5000 with `invalid_request`, and never calls the provider for invalid radii.
- Public nearby-food request JSON cannot control provider localization.
- Google upstream request JSON always contains `languageCode: "zh-TW"`.
- A valid non-empty provider fallback candidate remains available when zh-TW text is unavailable.

## Boundaries

- Tests are deterministic and use injected/fake collaborators; no real GPS, Google request, or external network.
- Public request/response and `IPlaceProvider.SearchAsync` shapes remain unchanged.
- RED evidence must fail only because production behavior is not implemented; syntax, setup, or assertion mistakes must be corrected before implementation begins.

# Document availability correction - 2026-10-05

## Online STEP 3D coverage

STEP archives are checked in the same bounded asynchronous audit against the
existing Drawing/{modelId}_stp.zip download source. HTTP 200 with ZIP or binary
content type confirms Online; 404/410 confirms Missing; other failures remain
Unverified. A local archive remains Offline. No ZIP is downloaded during the
audit. UI, print and confirmed-missing filter share the same status.

## Compact badges and English fallback

Commercial-sheet cells now display green/red language badges in a fixed-height
104-pixel row rather than fifteen text lines. Green means confirmed Online or
Offline; red means Missing or Unverified (the tooltip distinguishes them).
Printing uses the same badges and print color adjustment.

Direct languages: DA, DE, EN, FR, IT, NL, PL, SV. Per user policy, BG, CS, HU,
IS, NO, RO and SL use EN only and are grouped under Fallback EN available when
English exists online or offline. These seven languages are not probed separately
and do not count as missing translations. The confirmed-missing filter never
counts an unverified response as missing. Test-DocumentCoverageBadges.ps1 tests
online/offline EN, missing EN and unverified EN against the compiled code.

The audit previously used local files only but labelled their absence Missing.
Commercial-sheet coverage now checks all 15 supported languages against the
same canonical site paths used by document retrieval, including customer and
canonical model names. It does not assume English fallback proves availability
in another language. The reviewed selection API has no document-boolean route.

The audit runs HEAD checks asynchronously, with at most four concurrent requests
and a four-second timeout per request. No PDF downloads are performed. HTTP 200
with PDF content type confirms Online; only 404/410 confirms Missing. Timeouts,
connection errors, HTML responses and unsupported HEAD remain Unverified.
Local copies remain Offline irrespective of network failures.

UI and printable output show status per language. Other categories without an
online resolver explicitly show local absence or Online unverified, never a
global Missing claim. The confirmed-missing filter applies to commercial-sheet
languages only. Closing the audit cancels pending work; startup is unaffected.

Verification: AV/x86 solution compiled to the isolated document-online-build
folder. The actual compiled probe returned Online for SG 127 FS French
(FS_127_FR_AV.pdf, HTTP 200 application/pdf), Missing for a deliberately absent
test filename and Unverified for an unavailable loopback endpoint.

The previously exported temporary HTML is a historical snapshot and is not
rewritten. Open the updated audit and print again after online checks complete.
Installer publication and standard-binary replacement remain separate gates.

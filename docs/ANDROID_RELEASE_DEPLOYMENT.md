# Android Play Console Release

`SpaceDodger.Android/DeployAndFix/BuildAndSign.bat` creates the signed Android
App Bundle for Google Play. It clears Release intermediates, builds an AAB,
signs it with the local SpaceDodger keystore, and writes the deliverable to:

```text
SpaceDodger.Android/bin/Release/signed/com.arargames.spacedodger-SIGNED.aab
```

## Automatic versioning

Space Dodger follows the same hands-free release versioning convention as
Blocked. `BuildAndSign.bat` derives both values from the build date and time:

| Package field | Generation |
| --- | --- |
| `versionCode` | final year digit + `MMddHHmm` |
| Display version | `1.0.yyyy.MM.dd` |

The script asks only for the keystore password; it never prompts for either
version field. This keeps the release flow identical to the established Blocked
workflow.

Google Play can show an informational warning when the first generated code is
substantially larger than an older manually assigned code. Review the generated
code printed by the script and use Play Console's **Proceed anyway** option
when the uploaded package is the intended release.

## Signing prerequisites

`CreateKeystore.bat` is only for creating the first local keystore. Once a
bundle has been uploaded to Google Play, preserve the keystore and alias: every
future update must use the same upload signing identity. `BuildAndSign.bat`
prompts for the keystore password and never stores it in the repository.

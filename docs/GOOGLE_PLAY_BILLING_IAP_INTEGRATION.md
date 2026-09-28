# Google Play Billing & In-App Purchase (IAP) Integration

## 1. Overview & Google Play Console Prerequisites

To activate in-app products (One-time products / Tek seferlik ürünler) on the Google Play Console:
1. The app package (AAB / APK) must declare the billing permission in its manifest:
   ```xml
   <uses-permission android:name="com.android.vending.BILLING" />
   ```
2. Uploading a build containing this permission to **Internal testing** or **Closed testing** immediately unlocks the **"Create product"** button in Google Play Console (*Monetize with Play -> Products -> One-time products*).

## 2. In-App Product Catalog

The products configured in the application (`ArarGamesApplications.cs` & `SupportCreditsScreen.cs`):

| Product ID | Type | Purpose | Behavior on Purchase |
| --- | --- | --- | --- |
| **`remove_ads`** | Non-consumable (Tek seferlik kalıcı) | Removes all banner ads and interstitial ads | Acknowledged via Google Play Billing. `Context.Save.Data.AdsRemoved = true` persisted. Banner hidden immediately. Restorable on new devices. |
| **`buy_me_a_coffee`** | Consumable (Tüketilebilir) | Developer tip / support package | Consumed via `BillingClient.Consume` so players can purchase again anytime. |

## 3. Architecture & Implementation (SOLID / DIP)

- **`IPlatformServices`:** Declares platform-neutral `PurchaseConsumable(productId)`, `PurchaseNonConsumable(productId)`, and `RestorePurchases()`.
- **`DesktopPlatform`:** Provides clean no-op default implementations. Completely free of Android / Play Billing dependencies.
- **`AndroidIAPService` (`SpaceDodger.Android`):**
  - Uses official **Google Play Billing Library v8 (`Xamarin.Android.Google.BillingClient` 8.1.0)**.
  - Implements `IPurchasesUpdatedListener` and `IBillingClientStateListener`.
  - Caches queried product details safely using `ConcurrentDictionary<string, ProductDetails>` to eliminate concurrency race conditions.
  - Handles `BillingResponseCode.ItemAlreadyOwned` gracefully by restoring entitlements rather than showing failure errors.
  - Acknowledges non-consumables within the 3-day refund window.
  - Automatically queries active purchases on `RestorePurchasesAsync()` to restore the Remove Ads status on app reinstall or device migration.

## 4. Building & Publishing Signed AAB for Closed Test

To upload the new build to Google Play Console:
1. Run `SpaceDodger.Android/DeployAndFix/BuildAndSign.bat`.
2. Enter keystore password when prompted.
3. The script automatically derives a strictly increasing `versionCode` from the date (`yyMMddHHmm`) and packages an optimized AAB signed with `SpaceDodger-Key.keystore`.
4. Output file:
   ```text
   SpaceDodger.Android/bin/Release/signed/com.arargames.spacedodger-SIGNED.aab
   ```
5. Upload this `.aab` to Google Play Console under **Closed testing** or **Internal testing**. Once processed, the **One-time products** page will allow creating `remove_ads` and `buy_me_a_coffee`.

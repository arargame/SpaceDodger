# Google Play Store Listing Video & API Batch Synchronization Guide

## 1. Overview
This document specifies how the official 90-second YouTube store trailer (`https://www.youtube.com/watch?v=MUCJ-uLL5cg`) is synchronized across all 74 localized Google Play Store listings for **Space Dodger** (`com.arargames.spacedodger`).

---

## 2. Store Video Architecture in Google Play Console

### Key Concept: Default Inheritance
In Google Play Console architecture:
1. **Main Store Listing (Default / en-US):**
   - The field **"Promo video" (Tanıtım videosu)** located under *Store presence > Main store listing > Graphics* accepts a standard YouTube URL.
2. **Automatic Multi-Language Propagation:**
   - When a YouTube video URL is set on the **Main store listing**, **Google Play automatically serves this promotional video across ALL 65+ supported languages and translations**.
   - Individual language translations inherit the main video unless explicitly overridden.
   - Therefore, simply pasting `https://www.youtube.com/watch?v=MUCJ-uLL5cg` once into the Main Store Listing updates the store vitrine globally.

---

## 3. Automated API Batch Synchronization (`androidpublisher` v3)

For developers wishing to push localized titles, descriptions, and the video URL explicitly to all 74 translations via Google Play's REST API:

### A. Prerequisites
1. **Google Play Console API Access:**
   - Navigate to *Google Play Console > Developer account > API access*.
   - Create or link a Google Cloud Project and generate a **Service Account**.
   - Grant the Service Account the **"Manage store presence"** (Mağaza varlığını yönetme) permission.
   - Download the private key JSON file (e.g. `play-service-account.json`).
2. **Python Environment:**
   ```bash
   pip install google-api-python-client google-auth
   ```

### B. Execution Script (`tools/upload_store_listings_api.py`)
Run the automated script to batch-upload all 74 languages in a single edit transaction:

```bash
python tools/upload_store_listings_api.py --key path/to/play-service-account.json
```

To test without committing:
```bash
python tools/upload_store_listings_api.py --key path/to/play-service-account.json --dry-run
```

---

## 4. Local Listing Datasets Updated
All datasets have been recompiled with the official video link:
- **`tools/generate_store_listings.py`:** `VIDEO_URL = "https://www.youtube.com/watch?v=MUCJ-uLL5cg"`
- **`docs/SpaceDodger_Store_Listings_All.json`:** 74 locales compiled with localized metadata and trailer link.
- **`docs/SpaceDodger_Store_Listings_All.csv`:** Full CSV export ready for manual import or external tooling.
- **`docs/GOOGLE_PLAY_STORE_LISTING_EN.md`:** Updated metadata specification.
- **`docs/STORE_TRAILER_90S_SPEC.md`:** Technical video and EDL profile with official video URL.

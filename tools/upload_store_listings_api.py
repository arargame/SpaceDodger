# -*- coding: utf-8 -*-
"""
Space Dodger - Google Play Store Listings API Batch Uploader
Uploads all 74 localized store listings (Titles, Descriptions, Promo Video)
to Google Play Console via the Google Play Developer Publishing API (androidpublisher v3).

Prerequisites:
    pip install google-api-python-client google-auth

Usage:
    python tools/upload_store_listings_api.py --key path/to/service-account.json
"""

import argparse
import json
import os
import sys

PACKAGE_NAME = "com.arargames.spacedodger"
DEFAULT_JSON = os.path.join(os.path.dirname(__file__), "..", "docs", "SpaceDodger_Store_Listings_All.json")


def upload_listings(key_file, listings_json=DEFAULT_JSON, target_langs=None, dry_run=False):
    if not os.path.isfile(key_file):
        print(f"[ERROR] Service account key file not found: {key_file}")
        print("\nGoogle Play Developer API'yi kullanabilmek için Google Play Console'dan")
        print("bir 'Service Account (Hizmet Hesabı)' JSON anahtarı indirip yolunu belirtmeniz gerekir.")
        print("Örnek: python tools/upload_store_listings_api.py --key play-service-account.json --langs en-US,de-DE,it-IT")
        sys.exit(1)

    try:
        from google.oauth2 import service_account
        from googleapiclient.discovery import build
    except ImportError:
        print("[ERROR] Required libraries missing. Please run:")
        print("    pip install google-api-python-client google-auth")
        sys.exit(1)

    with open(listings_json, "r", encoding="utf-8") as f:
        listings = json.load(f)

    if target_langs:
        # Match either exact code like 'en-US' or prefix like 'en', 'de', 'it'
        req_langs = [l.strip().lower() for l in target_langs.split(",") if l.strip()]
        listings = [
            item for item in listings
            if item["language"].lower() in req_langs or any(item["language"].lower().startswith(r) for r in req_langs)
        ]

    print(f"[INFO] Processing {len(listings)} localized listing(s)...")
    print(f"[INFO] Authenticating for package: {PACKAGE_NAME}...")

    credentials = service_account.Credentials.from_service_account_file(
        key_file,
        scopes=["https://www.googleapis.com/auth/androidpublisher"]
    )
    service = build("androidpublisher", "v3", credentials=credentials)

    print("[INFO] Creating new Google Play edit session...")
    edit_request = service.edits().insert(packageName=PACKAGE_NAME, body={})
    edit = edit_request.execute()
    edit_id = edit["id"]
    print(f"[INFO] Edit session created with ID: {edit_id}")

    success_count = 0
    fail_count = 0

    for item in listings:
        lang = item["language"]
        body = {
            "title": item["title"],
            "shortDescription": item["shortDescription"],
            "fullDescription": item["fullDescription"],
            "video": item.get("video", "")
        }

        print(f"  -> Uploading [{lang}]: {item['title']} (Video: {body['video'][:35]}...)")
        if dry_run:
            success_count += 1
            continue

        try:
            service.edits().listings().update(
                packageName=PACKAGE_NAME,
                editId=edit_id,
                language=lang,
                body=body
            ).execute()
            success_count += 1
        except Exception as ex:
            print(f"     [WARN] Failed to update {lang}: {ex}")
            fail_count += 1

    if dry_run:
        print(f"\n[DRY RUN] Tested {success_count} listings. Edit not committed.")
        return

    print(f"\n[INFO] {success_count} listings uploaded successfully ({fail_count} failed).")
    print("[INFO] Committing changes to Google Play Console...")

    try:
        commit_request = service.edits().commit(packageName=PACKAGE_NAME, editId=edit_id)
        commit_request.execute()
        print("[SUCCESS] All targeted store listings successfully published to Google Play!")
    except Exception as ex:
        print(f"[ERROR] Failed to commit edit session: {ex}")
        sys.exit(1)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Upload Space Dodger localized listings to Google Play Console API.")
    parser.add_argument("--key", required=False, default="play-service-account.json",
                        help="Path to Google Play Developer API Service Account JSON key.")
    parser.add_argument("--langs", required=False, default=None,
                        help="Comma-separated language codes to upload (e.g. 'en-US,de-DE,it-IT' or 'en,de,it'). Defaults to all.")
    parser.add_argument("--json", required=False, default=DEFAULT_JSON,
                        help="Path to SpaceDodger_Store_Listings_All.json")
    parser.add_argument("--dry-run", action="store_true",
                        help="Validate listings and create edit without committing.")

    args = parser.parse_args()
    upload_listings(args.key, args.json, args.langs, args.dry_run)

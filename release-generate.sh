#!/bin/bash

# Ensure you have the latest tags locally
git fetch --tags

# Loop through every tag
for tag in $(git tag); do
  # Check if a release already exists for this tag
  if gh release view "$tag" &>/dev/null; then
    echo "✅ Release for $tag already exists — skipping."
  else
    echo "🚀 Creating release for $tag..."
    gh release create "$tag" \
      --verify-tag \
      --title "$tag" \
      --notes "Release for exercise $tag"
  fi
done
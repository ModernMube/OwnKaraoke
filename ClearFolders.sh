#!/bin/bash

# We set the search starting directory (where the script runs)
search_path="$(cd "$(dirname "$0")" && pwd)"

echo ""
echo "======================================================="
echo "Start: Clean up folder at the following location:"
echo "$search_path"
echo "======================================================="
echo ""

# -----------------------------------------------------------
# 1. bin folders delete
# -----------------------------------------------------------
echo "search and delete bin folders..."
find "$search_path" -type d -name "bin" -print0 | while IFS= read -r -d '' dir; do
    echo "Delete: $dir"
    rm -rf "$dir"
done

echo ""

# -----------------------------------------------------------
# 2. obj folders delete
# -----------------------------------------------------------
echo "search and delete obj folders..."
find "$search_path" -type d -name "obj" -print0 | while IFS= read -r -d '' dir; do
    echo "Delete: $dir"
    rm -rf "$dir"
done

echo ""
echo "======================================================="
echo "Cleaning completed!"
echo "======================================================="
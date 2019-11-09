#!/bin/bash -e

echo "installing tools"
pip install mkdocs mkdocs-material pygments pymdown-extensions recommonmark
echo "installed"

echo "generating new documentation"
mkdocs build
echo "documentation generated"

echo "publishing to github"

echo "published"
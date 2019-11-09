#!/bin/bash -ex

echo "installing tools"
pip install --user mkdocs mkdocs-material pygments pymdown-extensions recommonmark
echo "installed"

echo "generating new documentation"
mkdocs build
echo "documentation generated"

echo "publishing to github"

echo "published"
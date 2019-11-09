#!/bin/bash -ex

echo "installing tools"
pip install --user mkdocs mkdocs-material pygments pymdown-extensions recommonmark
echo "installed"

ls -alh
git worktree list
git worktree add site gh-pages

echo "generating new documentation"
mkdocs build
echo "documentation generated"

echo "publishing to github"
cd site
git add --all
git commit -m "documentation update"
git push origin gh-pages
cd ..
git worktree remove site
echo "published"

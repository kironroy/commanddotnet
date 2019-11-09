#!/bin/bash -ex

echo "installing tools"
pip install --user mkdocs mkdocs-material pygments pymdown-extensions recommonmark
echo "installed"

echo "generating new documentation"
rm -rf site
mkdocs build
echo "documentation generated"

echo "publishing to github"
git worktree add --track -b site gh-pages
cd site
git add --all
git commit -m "documentation update"
git push origin gh-pages
cd ..
git worktree remove site
echo "published"

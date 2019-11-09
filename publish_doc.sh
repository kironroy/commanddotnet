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

//push setup
git config --global user.email "travis@travis-ci.com"
git config --global user.name "Travis CI"
git config --global push.default current

echo "publishing to github"
cd site
git add --all
git commit -m "documentation update"
git push https://${GITHUB_TOKEN}@github.com/bilal-fazlani/commanddotnet.git origin gh-pages
# git push origin gh-pages
cd ..
git worktree remove site
echo "published"

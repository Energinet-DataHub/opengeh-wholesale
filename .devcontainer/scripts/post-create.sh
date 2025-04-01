#!/usr/bin/env bash

WORKSPACE_DIR=${1:-"."}

# find all .tool-versions within the repo, but ignore all hidden directories
if test -f "$WORKSPACE_DIR/.tool-versions"; then
  echo "asdf setup for $WORKSPACE_DIR"

  # install all required plugins
  plugins=$(cat $WORKSPACE_DIR/.tool-versions | cut -d' ' -f1 | grep "^[^\#]")
  for plugin in $plugins; do
    asdf plugin add $plugin
  done

  # Go to the workspace directory
  pushd $WORKSPACE_DIR
    # install all required versions
    asdf install

    # reshim all versions
    asdf reshim
  popd
fi

#!/bin/bash

for tag in $(git tag); do
  echo "- [$tag](https://github.com/Aulanko/KubernetesSubmissions/releases/tag/$tag)"
done
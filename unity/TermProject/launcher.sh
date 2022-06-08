#!/bin/bash

mkdir -p demos
rm -rf demos/*
for ((i=1; i<=$1; i++))
do
    echo "Instance $i"
    cp -r demo_1.app demos/demo_${i}.app
#    open -a demos/demo_${i}.app
    ./demos/demo_${i}.app/Contents/MacOS/TermProject -screen-width 800 -screen-height 600 &
    sleep 3
done
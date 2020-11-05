#!/bin/sh

curl -F file=@test-1.html http://localhost:5000/Pdf/Upload -o test-1.pdf

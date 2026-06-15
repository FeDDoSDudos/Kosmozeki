@echo off
cd /d "%~dp0"
cd ..
cd ..
tree /a /f > ".\Dummy\tree\project-tree.txt"

# Ark Save Auto Backup
This tool will automatically backup your ark save files. It backs up your .ark, .arktribe, and .arkprofile files to a directory of your chosing. It will keep 20 backups of each file.

## Installation

It's a portable exe. You can put it wherever you want, and just run it when you want to. You can create a shortcut in your startup folder to have it start when you log in to your computer.

## Usage

Select the folder where your ark saves are. This is usually `C:\Program Files (x86)\Steam\steamapps\common\ARK\ShooterGame\Saved\SavedArksLocal`. Select the folder you want to keep your backups.Then enable backup action by checking the checkbox.

If you minimize or close the window, it will minimize to the taskbar.

To close the application, right click it's icon in the system tray and select close.

## Restoring a backup

First navigate to the backups directory you selected, and open the folder corresponding to the file you want to restore. Optionally, you might want to copy all the files to another location until the restoration process is finished, as restoring the backup will backup the backup. Right click on the file you want to restore, and click copy. The filenames will appear like `TheIsland.ark_bak_2020.07.29.12.12.38.156`

Go to your ark save folder in your steamapps. Right click in the folder and click paste.

Delete the corrupt/bad save file you are replacing.

Right click on the backup file and click rename. Remove `_bak_` and everything after that. You should be left with `TheIsland.ark` from the above example.

### Other info

Each file is backed up into a subdirectory of the original file's name. When each file is copied from the ark folder, a suffix is appended. The numbers after `_bak_` are _year.month.day.hour.minute.second.thousandths_

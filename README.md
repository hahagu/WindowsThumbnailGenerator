# Windows Thumbnail Generator
This application aims to recreate the missing feature of folder thumbnails in Windows 11.

It uses ImageMagick to composite media thumbnails in a folder to create a thumbnail, and generates a desktop.ini file to set it as the thumbnail.

## Usage
The usage is simple.

1. Download the application from the <a href="https://github.com/hahagu/WindowsThumbnailGenerator/releases">Releases</a> page
2. Start up `Thumbnail Generator.exe`
3. Choose a directory
4. Choose whether to generate thumbnails recursively (If checked, the application will create thumbnail icons for subfolders as well.)
5. Press start and wait.

The application will automatically generate thumbnails and place them in correct directories.

## Troubleshooting
### Thumbnails not updating
If thumbnails are not updating, it is most likely the thumbnail cache not being properly updated.

Therefore, you need to reset the thumbnail cache.

To do this, you have to open up the `Disk Cleanup` utility, check the box for Thumbnails, and start cleaning.

This will effectively delete all folder icon cache, so they will be forced to be updated.

### Folders containing special characters
The `desktop.ini` file by default, is always made in the native language encoding that your Windows is in.

However, when we are manually writing the `desktop.ini` file, the encoding is by default, UTF-8.

Therefore, your system needs to support UTF-8.

To enable the UTF-8 support, open Settings > Time & Language > Language & Region > Administrative language settings,

Open the Administrative tab, click on change system locale, and check the box that reads `Beta: Use Unicode UTF-8 for worldwide language support.

After restarting the PC, generate the thumbnails again, and the thumbnails should work.

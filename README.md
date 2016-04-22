# GCI-Data-Usage

This is the source for the GCI Data Usage App.  Free to use, edit and redistribute  Enjoy!

# Improvements in this fork

* Create missing solution.  No installer project yet.
* Include icon files and move to embedded resources
* Swapped out embedded browser for ScrapySharp and HtmlAgilityPack.  I just discovered this library and it is much easier than manual response/request.  This may be my new goto package for scraping.
* Refactored code into classes so it can be reused easily.  I may try creating a web api interface somewhere.
* Add expected usage to tooltip--requires setting the reset day at the moment, but this could be determined from the page.  This may be too much text if wireless is included, but I don't use it.
* Added scraping of the detail page and included a detail view.  The format is set to output the way YAMon usage reporting tool can use, but I also included the past year history since it is on the page.
* Use timer control instead of a sleeping thread.  Probably no performance difference, but I prefer stopping a timer to aborting a thread.

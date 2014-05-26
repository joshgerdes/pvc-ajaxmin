![PVC Build Engine](http://i.imgur.com/vyROdJJ.png)

pvc-ajaxmin
===========

This is a plugin to utilize the Microsoft Ajax Minifier with the [PVC Build Engine](https://github.com/pvcbuild).

###Parameter Options

The plugin has the ability to take in command line switches for the Microsoft Ajax Minifier. More details on what can be passed in can be found [here](http://ajaxmin.codeplex.com/wikipage?title=Command-Line%20Switches).

###Usage Examples

Basic:

```
pvc.Source("js/*", "css/*", "test.js", "test.css")
	.Pipe(new PvcAjaxmin())
	.Save(@"deploy");
```

With Parameters:

```
pvc.Source("css/*")
	.Pipe(new PvcAjaxmin("-colors:hex"))
	.Save(@"deploy");
```
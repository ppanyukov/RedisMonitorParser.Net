# RedisMonitorParser.Net

A very simple parser for Redis monitor output for .NET

## Nuget package

The nuget package is here: <https://www.nuget.org/packages/RedisMonitorParser/>

## Redis monitor

Redis <http://redis.io> has `MONITOR` command. The output of it looks like this:


    [timestamp]-------[db client_IP_port]-[command]-[0-N arguments]

	1424875375.201784 [0 [::]:11707] "GET" "FOOBAR"
	1424875771.497401 [0 [fe80::]:11759] "GET" "FOOBAR"
	1424878505.439931 [0 127.0.0.1:12202] "client" "getname"
	1424878538.224125 [0 127.0.0.1:12202] "client" "list"
	1424880076.504896 [0 127.0.0.1:12461] "subscribe" "foo"
	1424880093.015950 [0 127.0.0.1:12462] "unsubscribe" "foo"
	1424880096.354794 [0 127.0.0.1:12462] "unsubscribe" "foo"
	1424883930.897944 [0 127.0.0.1:12462] "get" "foo bar"
	1424886119.428621 [0 127.0.0.1:12462] "get" "KEY2"
	1424886131.432880 [0 127.0.0.1:12462] "get" "KEY2"


## The parser

This parser provides programmatic access to the output of the `MONITOR` command with a minimum of fuss.

At the moment there is only one parser:

1. `Parser.ParseRaw()` which just gives acess to the individual fields with values of these fields as string and not modified in any way.


## Example

Given Redis MONITOR produced the following line:

	
	1424886910.012273 [0 127.0.0.1:12462] "MGET" "KEY \" 1" "K\\EY2" "KEY3";


To parse:

	var parser = new RedisMonitorParser.Parser();
	var parsedLine = parser.ParseRaw(inputLine);

The parsed line will return an instance of `RawMonitorLine` with the following properties:

	- RawLine -> 1424886910.012273 [0 127.0.0.1:12462] "MGET" "KEY \" 1" "K\\EY2" "KEY3";
	- RawTimeStamp -> 1424886910.012273
	- RawDb -> 0
	- RawCommand -> MGET
	- RawArgs -> array of 
			[
				KEY " 1, 
				K\EY2, 
				KEY3
			]

NOTE:

- All values of fields are `strings`, and `RawArgs` is `string[]`.
- The Redis-encoded escaped strings (such as `\\` will get unescaped).
- The quotes around the command and arguments are stripped.



## The raw parser features

- Pretty reliable.

- Leaves all values of the fields untouched except for one case where it strips quotes from around the command name and the arguments.

- Gives access to:

    - timestamp
    - db number
    - command name
    - list of arguments


- Reliably parses arguments with spaces, embedded quotes and the like. 

- Now full support for Redis-encoded binary string and escaped chars, e.g.
  these will get parsed and unescaped correctly: `\xff`, `\t` `\n`, `\\` and so on.


## Limitations

- Assumes the first quoted word after the db/IP address block to be the command name. There are very few rare commands in Redis which are composed of more than one word. For these, only the first word would be recognised as a command. The remaining words would be parsed as arguments. Everything should still work correctly but the client application would need to reconstruct the exact full command using a combination of command and args.
- No access to client IP:port yet (because there can be a mix of IPv4 and IPv6 addresses).
- No parsing of UNIX timestamp into any .NET date/time types, e.g. `DateTime` (yet).
- May not work with versions of `MONITOR` output produced by redis versions earlier than 2.6.
- Limited tests with the arguments containg Redis-encoded binary strings, may still be bugs there.





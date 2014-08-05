AzureTests
==========

Repo to play around with Windows Azure


In the solution: 
==


A basic Tcp Server that reads and writes to the same client's stream.


Tested as a worker role on Azure.

To deploy, pretty straight forward. Use the staging environment (or not ;) ) and open putty / telnet to the [ip] [port] currently set in the default endpoint. 


The Azure Config files need to be re-added. This will affect the endpoint configuration as well. 

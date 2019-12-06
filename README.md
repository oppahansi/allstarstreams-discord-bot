# Allstarstreams Discord Bot

## Config
See config_files folder for examples.
  
## Commands
In this order:
command | command alias

I am using the "." as command prefix, this can be edited in the config

Basically only mod and mod+ commands have aliases, except help command

* verificationcode | vc  
    * Updates a verification code.   
    * .vc type code  
    * Types: vaderstreams, lightstreams, holodisc , beasttv
* specialchan | sc  
    * Flags a channel with a channel type.  
    * .sc channel channelType  
    * ChannelTypes: log, announce, tickets, archive, releases  
* rspecialchan | rsc  
    * Unflags a special channel.  
    * .rsc channelType  
* cmdchan | cc  
    * Binds a command to a channel  
    * .cc command channel  
    * Command be bound to multiple channels  
* rcmdchan | rcc  
    * Unbinds a command from a channel  
    * .rcc command channel  
    * channel is optional and if left out command will be unbound from ALL channels  
* cmdrole | cr  
    * Binds a command to a minimum required role  
    * .cr command roleName  
    * A command can only be bound to one minimum required Role, order of commands in discord important.   
* rcmdrole | rcr  
    * Unbinds a command from a minimum required role  
    * .rcr command  
* minaccage | mac  
    * Sets minimum account age threshold for new users in discord  
    * .mac minAccAge  
    * .mac  
    * Minimum acc age is in hours, if left out, current settings will be printed out  
* archive | arc  
    * Enables / Disables archiving of deleted messages  
    * .arc 1  
    * 1 = enable, 0 = disable, a channel has to be flagged as archive channel  
* mail | m
    * Sends an email to soon to be expired accounts
    * .m provider expirationDate
    * Possible providers are: vader | v, lightstreams | l, beasttv | b
    * Expiration date expects this format: MM-dd-yyyy
* maildates | md
    * Sends an email to soon to be expired accounts within the dates range
    * .m provider startindDate endingDate
    * Possible providers are: vader | v, lightstreams | l, beasttv | b
    * Expiration dates expect this format: MM-dd-yyyy
* automessage | am
    * Sets an automessage for the given channel, posting every x minutes
    * .am channelMention timeInterval Message goes here
    * channelMention usually looks like #vader-general, timeInterval is in minutes
* rautomessame | ram
    * Removes an automessage from given channel
    * .ram channelMention
    * channelMention usually looks like #vader-general
* messageinterval | mi
    * Sets a messages interval for given channel after which the automessage will be posted
    * mi channelMention amountOfMessages
    * channelMention usually looks like #vader-general
    * automessage timeinterval is reset if message amount was reached and message was posted
* lautomessage | lam
    * Shows currently set auto messages
    * .lam
* accifno | ai,
    * Get vaderstreams or lightstreams account info
    * .ai accIdOrName
* expired | exp  
    * Get a list of expired accounts for given provider.
    * .exp provider days
    * .exp provider
    * Possible providers are: vader | v, lightstreams | l, beasttv | b
    * If days is a negative number it will check for expired acc in the X last days
    * If days is a positive number greater than 0 it will check for acc expiring in the X next days
    * If days is 0, then it checks which accs expire today
    * works for vader, lightstreams is weird data
* disabled | dis   
    * Get a list of disabled accounts for given provider.
    * .dis provider
    * Possible providers are: vader | v, lightstreams | l, beasttv | b
* cmdcd | ccd  
    *  Sets a cooldown for a command  
    * .ccd command cooldown  
    * .ccd command  
    * Cooldwod is in seconds, if left out, current cooldown value will be printed out  
* specialchanlist | scl  
    * Prints a list of currently flagged channels  
    * .scl  
* cmdpermissions | cp  
    * Prints a list of none default command bindings  
    * .cp  
* announce | ann  
    * Posts an announcement in the announcement channel  
    * .ann message  
    * Message has to be less than 2000 characters long, A channel has to be flagged as announcement channel  
* mute | m  
    * Mutes a user  
    * .m userMention muteDuration muteReason  
    * Mute duration is in minutes, a role named "Muted" has to exist  
* unmute | um  
    * Unmutes a user  
    * .um userMention unmuteReason  
* mutes | ms  
    * Prints a list of currently active mutes  
    * .ms  
* helpadd | ha  
    * Adds a quick help tag  
    * .ha tag help description here  
    * Help description cannot be longer than 1023 characters.  
* helpdel | hd  
    * Deletes a quick help tag  
    * .hd tag  
* answer | a  
    * Answers a ticket  
    * .a ticketId  
    * .a ticketId optional message  
* close | c  
    * Closes a ticket  
    * .c ticketId  
    * .c ticketId optional message  
* list | l  
    * Prints a list of currently open tickets  
    * .l  
* open | o  
    * Prints information about a ticket in more details  
    * .o ticketId  
* purge | clean  
    * Deletes messages in a channel that are not older than 14 days.  
    * .clean  
    * .clean amount  
    * .clean amount deleteType  
    * .clean amount deleteTyoe deleteStrategy  
    * Amount: The optional number of messages to delete; defaults to 10  
    * Delete type: The type of messages to delete - Self, Bot, or All; defaults to ALL  
    * Delete strategy: The strategy to delete messages - BulkDelete or Manual; defaults to BulkDelete  
* verify
    * Assigns a verified role to user for given code.
    * .verify code
* submit  
    * Submits a ticket  
    * .submit Problem description  
    * Problem description cannot be longer than 2000 characters  
    * Command and message is deleted instantly after pressing enter, to not show any information provided in the ticket
* cancel  
    * Cancels currently open ticket
    * .cancel
* status  
    * Display current ticket status
    * .status
* update  
    * Updates currently open ticket with an update message
    * .update update message here
    * Update message cannot be longer than 1023 characters
* help | h  
    * Display quick help information for a given tag/keyword
    * .h tag
    * .h
    * If tag is left out, currently available tags/keyword list will be printed
* cat  
    * Posts a random cat picture
    * .cat
* dog  
    * Posts a random dog picture
    * .dog
* wp  
    * Gets a random wallpaper for a given tag / keyword
    * .wp tag
* cn  
    * Posts a random Chuck Norris joke
    * .cn
* ym  
    * Posts a random yo mamma joke
    * .ym
* anime  
    * Searches and posts an anime overview for the given search term
    * .anime searchTerm
* manga  
    * Searches and posts a manga overview for the given search term
    * .manga searchTerm
* urban  
    * Searches and posts an urban definition for the given search term
    * .urban searchTerm
* xkcd
    * Gets a random xkcd comic.
    * .xkcd  
* movie  
    * Searches and posts a movie overview for the given movie name
    * .movie movie name
* series  
    * Searches and posts a series overview for the given series name
    * .movie series name
* boobs
    * Posts a random boobs picture
    * .boobs
* butts
    * Posts a random butts picture
    * .butts
* rule34
    * Posts a random rule 34 picture for the given tag
    * .rule34 tag
	
	
Commands operating on the web might not work anymore.
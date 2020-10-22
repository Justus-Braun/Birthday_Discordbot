# Birthday_Discordbot

Code for setting up MySQL Database
~~~~sql

  #Create Database
  Create database Birthday_DiscordBot;
  use Birthday_DiscordBot;

  #Create Table Guilds
  CREATE TABLE guilds (
      guildID BIGINT UNSIGNED PRIMARY KEY,
      channelID BIGINT UNSIGNED,
      prefix VARCHAR(5) default "!"
  );

  #Create Table user
  create table user(
      ID int primary key auto_increment,
      userID bigint unsigned,
      birthday date,
      guild bigint unsigned,
      foreign key (guild) references guilds(guildID) on update cascade on delete cascade
  );

~~~~

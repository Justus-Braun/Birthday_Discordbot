# Birthday_Discordbot

Code for setting up MySQL database
~~~~sql

  # Create database
  Create database Birthday_DiscordBot;
  use Birthday_DiscordBot;

  # Create table guilds
  CREATE TABLE guilds (
      guildID BIGINT UNSIGNED PRIMARY KEY,
      channelID BIGINT UNSIGNED,
      prefix VARCHAR(5) default "!"
  );

  # Create table user
  create table user(
      ID int primary key auto_increment,
      userID bigint unsigned,
      birthday date,
      guild bigint unsigned,
      foreign key (guild) references guilds(guildID) on update cascade on delete cascade
  );

~~~~

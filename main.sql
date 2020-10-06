#Create Database
Create database Birthday_DiscordBot;
use Birthday_DiscordBot;

#Create Table Guilds
create table guilds(
	ID int primary key auto_increment,
    guildID int8(0),
    channelID int8(0) 
);

#Create Table user
create table user(
	ID int primary key auto_increment,
    userID int8(0),
    birthday date,
    guild int,
    foreign key (guild) references guilds(ID) on update cascade on delete cascade 
);

#Query through user table
select * from user;
select guildID from guilds;

delete from guilds where guildID = 


#drop database Birthday_DiscordBot;
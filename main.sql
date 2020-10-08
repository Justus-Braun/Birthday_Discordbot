#Create Database
Create database Birthday_DiscordBot;
use Birthday_DiscordBot;

#Create Table Guilds
create table guilds(
    guildID bigint unsigned primary key,
    channelID bigint unsigned 
);

#Create Table user
create table user(
	ID int primary key auto_increment,
    userID bigint unsigned,
    birthday date,
    guild bigint unsigned,
    foreign key (guild) references guilds(guildID) on update cascade on delete cascade 
);

#Query through user table
select * from user;
select * from guilds;



select * from user left join guilds on user.guild = guilds.ID;


#drop database Birthday_DiscordBot;
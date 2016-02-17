                            
                    ##########################################
                    #                                        #
                    #        //////  //////  ////// //       #
                    #            //     // //     //         #
                    #       /////  /////  //     //  //      #
                    #      //     //     //     //////       #
                    #     //     //     //////     //        #
                    #                                        #
                    ##########################################

                          Team: SharpNado (aka #Nado)

                          PPC deliverable 4, 10FEB16

                                    Membri:     
                                Enzo Stoico
                                Luca De Paulis
                                Jacopo Cavallo
                                Francesco Manfredi
                                Gianluca Scatena
                                Federico Tersigni
                                Daniele Campli

 
 .:: Manuale d'uso ::.

    * Istruzioni di installazione *
        Setup del database .............................................. [01]
        Esecuzione del package engine ................................... [02]
        Avvio dell'interfaccia grafica .................................. [03]

    * Utilizzo *
        Creazione albero ................................................ [04]
        Importazione di un file albero .................................. [05]
        Esportazione di un albero ....................................... [06]
        Calcolo su un albero ............................................ [07]
        Modifica di valori .............................................. [08]
        Aiuto ........................................................... [09]

    * Miscellanea *
        Contatti ........................................................ [10]
        Statistiche & Info .............................................. [11]


-------------------------------------------------------------------------------------

                            * Istruzioni di installazione *


[01] - Setup del database

    1)  Avviare MSSQL server;
    2)  Eseguire dbSetup.exe; questo eseguibile crea il database "metadata" all'in-
        terno del server.



[02] - Installazione del package engine

    3)  Lanciare l'eseguibile engine.exe; questo mette in ascolto sulla porta 8888
        della macchina il server dell'engine.



[03] - Avvio dell'interfaccia grafica

    4)  Lanciare il file gui.exe; si aprirà una finestra dalla quale eseguire le
        connessioni con un'engine server ed un database; nel caso in cui si voglia
        testare il sistema nella sua interezza sulla propria macchina, inserire nel-
        le form i seguenti parametri:
            IP       -> 127.0.0.1
            port     -> 8888
            DB IP    -> local
            username -> user
            password -> pass
        I campi username e password, nel caso in cui il database venga eseguito in
        locale, verrano ignorati, tuttavia le rispette form devono comunque essere
        necessariamente riempite con dati fittizi (eg. "user"/"pass");
        
    5)  Premere il pulsante "Connect"; se la connessione con l'engine e l'autenti-
        cazione con il database vanno a buon fine, apparirà una finestra con un al-
        bero, le cui foglie sono i bottoni per eseguire varie operazioni.

-------------------------------------------------------------------------------------

                                    * Utilizzo *
                                    
[04] - Creazione albero
    
        Premere il bottone "Crea Albero". Si aprirà una finestra dalla quale possiamo
        definire tutte le caratteristiche che deve avere l'albero che vogliamo creare.
        La form “Nome” serve a specificare il nome dell'albero. Nella form "SplitSize"
        l'utente deve inserire un numero intero che definisce il numero di figli che 
        ogni nodo deve avere. La form "Depth" serve a specificare quanti livelli dovrà
        avere l'albero. La parte sinistra della finestra, affianco alle form, sarà riem-
        pita con tutti gli attributi già definiti nel database. Al click su un singolo
        attributo verrà aggiornata la corrispondente parte destra della finestra. L'uten-
        te, attraverso le checkbox, può specificare se assegnare l'attributo ad un ar-
        co oppure ad un nodo. In base al dominio dell'attributo ("numeric" o "string"),
        specificato in alto, l'utente può inserire una stringa o un range numerico.


[05] - Importazione di un file albero
        
        Premere il bottone "Importa File". Si aprirà una finestra dalla quale si potrà
        scegliere il file da importare nel database. Dopo aver selezionato il file, pre-
        mere il bottone "Conferma".


[06] - Esportazione di un albero
        
        Premere il bottone "Esporta Albero". Si aprirà una finestra nella quale è presen-
        te un menù a tendina dove è possibile selezionare un albero. Una volta scelto,
        premere il bottone "Conferma". Il nome del file che verrà esportato seguirà la
        direttiva "nomealbero.exi".


[07] - Calcolo su un albero
        
        Premere il bottone "Calcolo su Albero". Si aprirà una finestra dalla quale
        poter selezionare gli attributi di nodi e archi compresi in un determinato
        path. Il calcolo che viene effettuato è una somma degli attributi selezionati.
        Dal menù a tendina “Seleziona Albero” specificare l'albero su cui si vuole ef-
        fettuare il calcolo. Nelle  form “Nodo Iniziale” e “Nodo Finale” si deve speci-
        ficare un intero che che corrisponde agli ID dei nodi. Se si inserisce il nume-
        ro 1 nella form “Nodo Iniziale” e 3 nella form “Nodo Finale” verrà effettuata 
        la  somma gli attributi dal nodo 1 al nodo 3. Gli ID dei nodi sono assegnati 
        con una visita in preorder. Vengono popolate le sezioni Archi e Nodi.
        In queste sezioni sono contenuti gli attributi, li si può selezionare e aggiun-
        gere al calcolo attraverso il bottone “Aggiungi al calcolo”.


[08] - Modifica di valori

        Questa finestra permette di modificare i valori degli attributi per i nodi e 
        gli archi compresi in un determinato path (Nodo Iniziale – Nodo Finale) che 
        si può specificare nella finestra. Dal menù a tendina “Seleziona Albero” 
        selezionare uno tra tutti gli attributi presenti nel Database. Nelle form 
        “Nodo Iniziale” - “Nodo Finale” si dovranno inserire degli interi che corri-
        spondono agli ID dei nodi. Se si inserisce il numero 1 nella form “Nodo Ini-
        ziale” e 3 nella form “Nodo Finale” verranno modificati gli attributi dal 
        nodo 1 al nodo 3. Appena selezionato l'albero vengono popolate le sezione Archi
        e Nodi con gli attributi corrispondenti. Da queste sezioni si può selezionare
        un attributo per assegnargli un nuovo valore in base al dominio specificato dal
        testo sovrastante (i.e. "numeric" oppure "string").


[09] - Aiuto

        Per ogni funzionalità sopra documentata, esiste un bottone "Aiuto" che fornisce
        indicazioni guida per procedere ad un'utilizzo corretto del sistema.

-------------------------------------------------------------------------------------

                                    * Miscellanea *
[10] - Contatti
        
        Per informazioni, scrivere all'indirizzo: sharpnado@cx.33mail.com


[11] - Statistiche & Info
        
        Sistemi operativi di sviluppo:  Windows 7 Home Edition; 
                                        Debian Linux 8 (Jessie);
                                        Sabayon Linux;
                                        Ubuntu Linux;
                                        Mac OS X Snow Leopard.

        Strumenti di sviluppo:          Microsoft MSSQL 2014;
                                        Visual Studio 2010/2012;
                                        

        Canali di condivisione:         Github;
                                        Slack;
                                        Sito web del team;
                                        
                                    
        Strumenti di sviluppo:          Sublime Text Editor;
                                        Vim;
                                        Yed;
                                        LibreOffice;
                                        Skype;
                                        Telegram;


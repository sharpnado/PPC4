   M S B A M L   `   `   `  ����  PPC    PPC.CalcoloSuAlberoWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  CalcoloSuAlberoWindow��5   	   $	��473��6'   $	��408��64    � 
ResizeMode$ NoResize=�6[   .��5      � $��	0,0,-8,-5q�6   .+�5   
   �� -   5         ��Name$ CalcAlbMainLabel��5   ;   $��39��6   $
��Left=�6    $��Calcolo su Albero��6U   $	��Top=�6n   $	��278��6�    �� TextAlignment$ Center=�6�   $��Calibri)�6�   $��18(�6�   $��62,2,0,0q�6�   �� 5   
   -   5         ��Name$ SelAlbCalLabel��5   e   $��Seleziona Albero��6   $��24��6+   $
��Left=�67   $��	12,44,0,0q�6R   $	��Top=�6}   $	��108��6�   �� 5   
   -   5         ��Name$ 
separator1��5   N   $��42��6   $
��Left=�6    $��	10,10,0,0q�6;   $	��Top=�6b   $	��380��6z   �� 5	   
   -   5         ��Name$ SelAlbCalcomboBox��5	   N   $��19��6   $
��Left=�6   $��
125,49,0,0q�6:   $	��Top=�6i   $	��141��6�   6�   �� 5
   
   -   5         ��Name$ CalcoloButton��5
   g   $��Effettua Calcolo��6   $��26��6,   $
��Left=�68   $��
10,403,0,0q�6S   $	��Top=�6~   $	��154��6�   6�   �� 5   
   -   5        $ HelpModButton��5   \   $
��Help��6   $��26��6    $
��Left=�6,   $��343,403,0,0q�6G   $	��Top=�6s   $��43��6�   6�   �� 5   
   $��Nodo Inizale��6   $
��Left=�6'   $��24��6B   $��	12,85,0,0q�6N   $	��Top=�6a   $��80��6y   �� 5   
   $��Nodo Finale��6   $
��Left=�6&   $��24��6A   $��
12,109,0,0q�6M   $	��Top=�6a   $��80��6y   �� 5   
   -   5         ��Name$ NodInizioListBox��5      $
��Left=�6,   $��24��6G   $��
125,85,0,0q�6S   	 �� TextWrapping$
	 Wrap=�6g   $	��Top=�6{   $	��141��6�   �� 5   
   -	   5        $ NodFineListBox��5      $
��Left=�6*   $��24��6E   $��125,109,0,0q�6Q   $
	 Wrap=�6f   $	��Top=�6z   $	��141��6�   � 5   
   $
��Left=�6   $	��251��6*   $��
10,147,0,0q�67   $	��Top=�6K   $	��267��6c   
 � ColumnDefinitions
 5      �� 5      $
��128* �6#   �� 5      $
��139* �6#   .+�5      �� ���	#FFF4F4F5$
��Left=�6)   $	��251��6D   
���   �$	��Top=�6`   $	��375��6x   $��
0,0,-108,0q�6�   $��2��6�   �� 5      ���	#FFF4F4F5$
��Left=�6)   $��28��6D   $��3,10,0,0q�6P   
���   �$	��Top=�6q   $	��125��6�   $��0��6�    J� RenderTransformOrigin$ 
0.52,0.179&�6�   �� 5      ���	#FFF4F4F5$
��Left=�6)   $��38��6D   
���   �$	��Top=�6_   $	��128��6w   $��1��6�   5      �� 5   
   -
   5         ��Name$ AttrArchiListbox��5      $
��Left=�6,   $	��214��6G   $��137,184,0,0q�6T   $	��Top=�6i   $	��129��6�   $ 1.682,0.482&�6�   6�   �� 5   
   -   5        $ AttrNodiListBox��5      $
��Left=�6+   $	��214��6F   $��
12,184,0,0q�6S   $	��Top=�6g   $	��127��6   6�   �� 5   
   -   5        $ LabelAttrNodi��5      $
��Nodi��6'   $
��Left=�66   $��32��6Q   $��
46,147,0,0q�6]   $	��Top=�6q   $��46��6�   �� 5   
   -   5        $ LabelAttrArchi��5      $��Archi��6(   $
��Left=�68   $��32��6S   $��178,147,0,0q�6_   $	��Top=�6t   $��53��6�   �� 5   
   -   5        $ AttributeLabel��5      $��
Attributo:��6(   $
��Left=�6=   $��49��6X   $��285,147,0,0q�6d   $	��Top=�6y   $	��115��6�   �� 5   
   -   5        $ NomAttrLabel��5      $��La��6&   $
��Left=�63   $��45��6N   $��271,175,0,0q�6Z   $	��Top=�6o   $	��120��6�   �� 5   
   -   5        $ AggiungiButton��5      $��Aggiungi al calcolo��6)   $
��Left=�6G   $��45��6b   $��269,244,0,0q�6n   $	��Top=�6�   $	��109��6�   $ 0.5,0.5&�6�   z�5       V� 5!      .~�5"      �� �� 5#       �� AngleX$ 0.503Q�6$   �� 5$      T� 5%       T� X$ 0.491Q�6)   5&      5(      � 5)   
   $
��Left=�6   $��93��6*   $��285,292,0,0q�66   $	��Top=�6K   $��84��6c   ��5*      �� 5+       �� ImageSource$ Images/shark.png��6    j� Stretch$ Uniform=�6<   5-      5.      5/      
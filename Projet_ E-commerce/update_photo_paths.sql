-- Script SQL pour mettre Ã  jour les chemins des photos dans la base de donnÃ©es
-- ExÃ©cuter ce script dans SQL Server Management Studio

USE [Projet_e-commerce]
GO

BEGIN TRANSACTION
GO
-- Update: /uploads/products/12193bb4-7d57-454b-9b92-8e7b3323e9e2_télécharger (6).jpg -> /uploads/products/prod_1_t_l_charger_6_.jpg
UPDATE variantes SET photo = '/uploads/products/prod_1_t_l_charger_6_.jpg' WHERE photo = '/uploads/products/12193bb4-7d57-454b-9b92-8e7b3323e9e2_télécharger (6).jpg';
GO
-- Update: /uploads/products/1eb0b201-e4f9-4120-a0cd-743f681422c8_black_soap.jpg -> /uploads/products/prod_2_black_soap.jpg
UPDATE variantes SET photo = '/uploads/products/prod_2_black_soap.jpg' WHERE photo = '/uploads/products/1eb0b201-e4f9-4120-a0cd-743f681422c8_black_soap.jpg';
GO
-- Update: /uploads/products/21e57117-378b-4403-8231-3b2962a754f8_maskArgan.jpg -> /uploads/products/prod_3_maskArgan.jpg
UPDATE variantes SET photo = '/uploads/products/prod_3_maskArgan.jpg' WHERE photo = '/uploads/products/21e57117-378b-4403-8231-3b2962a754f8_maskArgan.jpg';
GO
-- Update: /uploads/products/244abdfc-5160-42fa-ada9-c8ade0a5913f_argan_Oil2.jpg -> /uploads/products/prod_4_argan_Oil2.jpg
UPDATE variantes SET photo = '/uploads/products/prod_4_argan_Oil2.jpg' WHERE photo = '/uploads/products/244abdfc-5160-42fa-ada9-c8ade0a5913f_argan_Oil2.jpg';
GO
-- Update: /uploads/products/278301d6-c74f-4c70-a94a-c9541bee913b_The Do-It-All Oil_ The Essence Of Argan Oil.jpg -> /uploads/products/prod_5_The_Do_It_All_Oil_The_Essence_
UPDATE variantes SET photo = '/uploads/products/prod_5_The_Do_It_All_Oil_The_Essence_' WHERE photo = '/uploads/products/278301d6-c74f-4c70-a94a-c9541bee913b_The Do-It-All Oil_ The Essence Of Argan Oil.jpg';
GO
-- Update: /uploads/products/29b48b92-8b07-42e4-bba4-1ab7ab87d5dc_arganOil.jpg -> /uploads/products/prod_6_arganOil.jpg
UPDATE variantes SET photo = '/uploads/products/prod_6_arganOil.jpg' WHERE photo = '/uploads/products/29b48b92-8b07-42e4-bba4-1ab7ab87d5dc_arganOil.jpg';
GO
-- Update: /uploads/products/2b5d215f-3dc5-4f49-8818-266db032fc7f_date500g.jpg -> /uploads/products/prod_7_date500g.jpg
UPDATE variantes SET photo = '/uploads/products/prod_7_date500g.jpg' WHERE photo = '/uploads/products/2b5d215f-3dc5-4f49-8818-266db032fc7f_date500g.jpg';
GO
-- Update: /uploads/products/329efba8-6de6-4b33-9be8-4233f408fe00_foot logo2.jpg -> /uploads/products/prod_8_foot_logo2.jpg
UPDATE variantes SET photo = '/uploads/products/prod_8_foot_logo2.jpg' WHERE photo = '/uploads/products/329efba8-6de6-4b33-9be8-4233f408fe00_foot logo2.jpg';
GO
-- Update: /uploads/products/48a0bff3-2929-4482-a90b-d73790951f06_date1kg.jpg -> /uploads/products/prod_9_date1kg.jpg
UPDATE variantes SET photo = '/uploads/products/prod_9_date1kg.jpg' WHERE photo = '/uploads/products/48a0bff3-2929-4482-a90b-d73790951f06_date1kg.jpg';
GO
-- Update: /uploads/products/4a26c2b6-3e5f-4609-afe1-2db44bbd7d37_télécharger (6).jpg -> /uploads/products/prod_10_t_l_charger_6_.jpg
UPDATE variantes SET photo = '/uploads/products/prod_10_t_l_charger_6_.jpg' WHERE photo = '/uploads/products/4a26c2b6-3e5f-4609-afe1-2db44bbd7d37_télécharger (6).jpg';
GO
-- Update: /uploads/products/4f7dd1f3-ffdf-47f9-8238-35c238972d90_creme argan.jpg -> /uploads/products/prod_11_creme_argan.jpg
UPDATE variantes SET photo = '/uploads/products/prod_11_creme_argan.jpg' WHERE photo = '/uploads/products/4f7dd1f3-ffdf-47f9-8238-35c238972d90_creme argan.jpg';
GO
-- Update: /uploads/products/4fc16c16-1f6a-4395-96be-07403cad25bc_Moroccan Amlou.jpg -> /uploads/products/prod_12_Moroccan_Amlou.jpg
UPDATE variantes SET photo = '/uploads/products/prod_12_Moroccan_Amlou.jpg' WHERE photo = '/uploads/products/4fc16c16-1f6a-4395-96be-07403cad25bc_Moroccan Amlou.jpg';
GO
-- Update: /uploads/products/5186c15a-c2b0-4298-a3cf-9a41b79a7a76_creme argan.jpg -> /uploads/products/prod_13_creme_argan.jpg
UPDATE variantes SET photo = '/uploads/products/prod_13_creme_argan.jpg' WHERE photo = '/uploads/products/5186c15a-c2b0-4298-a3cf-9a41b79a7a76_creme argan.jpg';
GO
-- Update: /uploads/products/52e0ad15-086e-4afc-ac39-f0d3c8c53a58_maskArgan.jpg -> /uploads/products/prod_14_maskArgan.jpg
UPDATE variantes SET photo = '/uploads/products/prod_14_maskArgan.jpg' WHERE photo = '/uploads/products/52e0ad15-086e-4afc-ac39-f0d3c8c53a58_maskArgan.jpg';
GO
-- Update: /uploads/products/55faec0e-7573-410d-8bea-c984ec18e5d5_date500g.jpg -> /uploads/products/prod_15_date500g.jpg
UPDATE variantes SET photo = '/uploads/products/prod_15_date500g.jpg' WHERE photo = '/uploads/products/55faec0e-7573-410d-8bea-c984ec18e5d5_date500g.jpg';
GO
-- Update: /uploads/products/656c031d-d0ed-4db7-a376-6bcc8af264cf_black_soap.jpg -> /uploads/products/prod_16_black_soap.jpg
UPDATE variantes SET photo = '/uploads/products/prod_16_black_soap.jpg' WHERE photo = '/uploads/products/656c031d-d0ed-4db7-a376-6bcc8af264cf_black_soap.jpg';
GO
-- Update: /uploads/products/6b3e6b9b-ad5f-4026-80d3-1a856018dd9a_ Argan _Oil.jpg -> /uploads/products/prod_17__Argan_Oil.jpg
UPDATE variantes SET photo = '/uploads/products/prod_17__Argan_Oil.jpg' WHERE photo = '/uploads/products/6b3e6b9b-ad5f-4026-80d3-1a856018dd9a_ Argan _Oil.jpg';
GO
-- Update: /uploads/products/73706c88-610a-4e81-b59c-2ecd5e66e621_argan_Oil2.jpg -> /uploads/products/prod_18_argan_Oil2.jpg
UPDATE variantes SET photo = '/uploads/products/prod_18_argan_Oil2.jpg' WHERE photo = '/uploads/products/73706c88-610a-4e81-b59c-2ecd5e66e621_argan_Oil2.jpg';
GO
-- Update: /uploads/products/7481d5b4-1f98-4f06-8747-1ed3086929ff_date1kg.jpg -> /uploads/products/prod_19_date1kg.jpg
UPDATE variantes SET photo = '/uploads/products/prod_19_date1kg.jpg' WHERE photo = '/uploads/products/7481d5b4-1f98-4f06-8747-1ed3086929ff_date1kg.jpg';
GO
-- Update: /uploads/products/77cb9d58-b188-4adb-a713-ad64cf95610e_babysitting.jpg -> /uploads/products/prod_20_babysitting.jpg
UPDATE variantes SET photo = '/uploads/products/prod_20_babysitting.jpg' WHERE photo = '/uploads/products/77cb9d58-b188-4adb-a713-ad64cf95610e_babysitting.jpg';
GO
-- Update: /uploads/products/783e6c23-ad71-4e26-9de8-a37234203ca8_docivox.webp -> /uploads/products/prod_21_docivox.webp
UPDATE variantes SET photo = '/uploads/products/prod_21_docivox.webp' WHERE photo = '/uploads/products/783e6c23-ad71-4e26-9de8-a37234203ca8_docivox.webp';
GO
-- Update: /uploads/products/80eee271-18cc-4cd8-aeed-3de2eb55f34f_arganOil.jpg -> /uploads/products/prod_22_arganOil.jpg
UPDATE variantes SET photo = '/uploads/products/prod_22_arganOil.jpg' WHERE photo = '/uploads/products/80eee271-18cc-4cd8-aeed-3de2eb55f34f_arganOil.jpg';
GO
-- Update: /uploads/products/87ad4082-10dc-4066-ba3c-58dacf244fdf_argan oil face.jpg -> /uploads/products/prod_23_argan_oil_face.jpg
UPDATE variantes SET photo = '/uploads/products/prod_23_argan_oil_face.jpg' WHERE photo = '/uploads/products/87ad4082-10dc-4066-ba3c-58dacf244fdf_argan oil face.jpg';
GO
-- Update: /uploads/products/8a3d1b5f-5305-4033-80eb-9a5f0f009852_arganMasque200ml.jpg -> /uploads/products/prod_24_arganMasque200ml.jpg
UPDATE variantes SET photo = '/uploads/products/prod_24_arganMasque200ml.jpg' WHERE photo = '/uploads/products/8a3d1b5f-5305-4033-80eb-9a5f0f009852_arganMasque200ml.jpg';
GO
-- Update: /uploads/products/8fcbee50-c087-48a4-91ac-a98b17823f1f_Argan _Oil.jpg -> /uploads/products/prod_25_Argan_Oil.jpg
UPDATE variantes SET photo = '/uploads/products/prod_25_Argan_Oil.jpg' WHERE photo = '/uploads/products/8fcbee50-c087-48a4-91ac-a98b17823f1f_Argan _Oil.jpg';
GO
-- Update: /uploads/products/92dce8f1-819b-4f0a-a137-8577932e99bd_argan oil face.jpg -> /uploads/products/prod_26_argan_oil_face.jpg
UPDATE variantes SET photo = '/uploads/products/prod_26_argan_oil_face.jpg' WHERE photo = '/uploads/products/92dce8f1-819b-4f0a-a137-8577932e99bd_argan oil face.jpg';
GO
-- Update: /uploads/products/9cccb6da-5939-42e4-b7c2-977ed5751466_babysitting.jpg -> /uploads/products/prod_27_babysitting.jpg
UPDATE variantes SET photo = '/uploads/products/prod_27_babysitting.jpg' WHERE photo = '/uploads/products/9cccb6da-5939-42e4-b7c2-977ed5751466_babysitting.jpg';
GO
-- Update: /uploads/products/9e5e8646-6d73-4565-a7d4-c15205c9f894_Moroccan Amlou 🇲🇦 أملو المغربي.jpg -> /uploads/products/prod_28_Moroccan_Amlou_.jpg
UPDATE variantes SET photo = '/uploads/products/prod_28_Moroccan_Amlou_.jpg' WHERE photo = '/uploads/products/9e5e8646-6d73-4565-a7d4-c15205c9f894_Moroccan Amlou 🇲🇦 أملو المغربي.jpg';
GO
-- Update: /uploads/products/9f531fd6-b47c-4286-bb0c-6766bad915de_savon beldi noir.jpg -> /uploads/products/prod_29_savon_beldi_noir.jpg
UPDATE variantes SET photo = '/uploads/products/prod_29_savon_beldi_noir.jpg' WHERE photo = '/uploads/products/9f531fd6-b47c-4286-bb0c-6766bad915de_savon beldi noir.jpg';
GO
-- Update: /uploads/products/a9cca200-9cc2-425d-a9f3-ce3851487e7d_arganMasque200ml.jpg -> /uploads/products/prod_30_arganMasque200ml.jpg
UPDATE variantes SET photo = '/uploads/products/prod_30_arganMasque200ml.jpg' WHERE photo = '/uploads/products/a9cca200-9cc2-425d-a9f3-ce3851487e7d_arganMasque200ml.jpg';
GO
-- Update: /uploads/products/b43d395e-14b5-4d51-b9fe-2fc66d9b4553_The Do-It-All Oil_ The Essence Of Argan Oil.jpg -> /uploads/products/prod_31_The_Do_It_All_Oil_The_Essence_
UPDATE variantes SET photo = '/uploads/products/prod_31_The_Do_It_All_Oil_The_Essence_' WHERE photo = '/uploads/products/b43d395e-14b5-4d51-b9fe-2fc66d9b4553_The Do-It-All Oil_ The Essence Of Argan Oil.jpg';
GO
-- Update: /uploads/products/bb1c2c83-9ea9-47dd-9de6-541584deb69a_Moroccan Amlou 🇲🇦 أملو المغربي.jpg -> /uploads/products/prod_32_Moroccan_Amlou_.jpg
UPDATE variantes SET photo = '/uploads/products/prod_32_Moroccan_Amlou_.jpg' WHERE photo = '/uploads/products/bb1c2c83-9ea9-47dd-9de6-541584deb69a_Moroccan Amlou 🇲🇦 أملو المغربي.jpg';
GO
-- Update: /uploads/products/df04733f-fb65-47d3-9923-4ec74f679123_logoApp.png -> /uploads/products/prod_33_logoApp.png
UPDATE variantes SET photo = '/uploads/products/prod_33_logoApp.png' WHERE photo = '/uploads/products/df04733f-fb65-47d3-9923-4ec74f679123_logoApp.png';
GO
-- Update: /uploads/products/e60e9c06-4ea1-467b-b475-85d2cbb8e5fd_pexels-picturemechaniq-1749303.jpg -> /uploads/products/prod_34_pexels_picturemechaniq_1749303
UPDATE variantes SET photo = '/uploads/products/prod_34_pexels_picturemechaniq_1749303' WHERE photo = '/uploads/products/e60e9c06-4ea1-467b-b475-85d2cbb8e5fd_pexels-picturemechaniq-1749303.jpg';
GO
-- Update: /uploads/products/f2fa7dfd-5ca5-473c-8f8f-138107253745_aya.jpg -> /uploads/products/prod_35_aya.jpg
UPDATE variantes SET photo = '/uploads/products/prod_35_aya.jpg' WHERE photo = '/uploads/products/f2fa7dfd-5ca5-473c-8f8f-138107253745_aya.jpg';
GO
-- Update: /uploads/products/f824de33-c52e-4d34-9fa7-c3ab4f5ffebf_argan_oil.jpg -> /uploads/products/prod_36_argan_oil.jpg
UPDATE variantes SET photo = '/uploads/products/prod_36_argan_oil.jpg' WHERE photo = '/uploads/products/f824de33-c52e-4d34-9fa7-c3ab4f5ffebf_argan_oil.jpg';
GO
-- Update: /uploads/products/fdb29a92-e4ab-43eb-893c-4fc208feeffd_date500g.jpg -> /uploads/products/prod_37_date500g.jpg
UPDATE variantes SET photo = '/uploads/products/prod_37_date500g.jpg' WHERE photo = '/uploads/products/fdb29a92-e4ab-43eb-893c-4fc208feeffd_date500g.jpg';
GO
-- Update: /uploads/products/Gemini_Generated_Image_8rn95y8rn95y8rn9.png -> /uploads/products/prod_38_Gemini_Generated_Image_8rn95y8rn95y8rn9.png
UPDATE variantes SET photo = '/uploads/products/prod_38_Gemini_Generated_Image_8rn95y8rn95y8rn9.png' WHERE photo = '/uploads/products/Gemini_Generated_Image_8rn95y8rn95y8rn9.png';
GO
COMMIT TRANSACTION
GO

PRINT 'Mise Ã  jour des chemins terminÃ©e: 38 enregistrements modifiÃ©s'
GO

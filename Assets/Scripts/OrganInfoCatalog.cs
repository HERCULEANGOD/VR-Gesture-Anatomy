public static class OrganInfoCatalog
{
    public static string GetOverview()
    {
        return "Select an organ in the body to inspect its function, major structures, blood supply, and common clinical notes.\n\n" +
               "Current lab flow:\n" +
               "- Whole-body overview first\n" +
               "- Select an organ to focus it\n" +
               "- Use the controls panel to isolate, dissect-view, or tune interaction speeds\n\n" +
               "This knowledge is currently local. An LLM-backed provider can be added later for deeper explanations and Q&A.";
    }

    public static string GetInfo(OrganController.OrganType organType)
    {
        switch (organType)
        {
            case OrganController.OrganType.Heart:
                return "Function:\nPumps oxygenated and deoxygenated blood through the body.\n\n" +
                       "Key anatomy:\n- Right and left atria\n- Right and left ventricles\n- Septum\n- Valves: tricuspid, pulmonary, mitral, aortic\n- Great vessels: vena cavae, pulmonary artery, pulmonary veins, aorta\n\n" +
                       "Clinical notes:\nCoronary artery disease, valve disease, arrhythmia, heart failure.";

            case OrganController.OrganType.Brain:
                return "Function:\nControls cognition, movement, sensation, language, memory, and autonomic regulation.\n\n" +
                       "Key anatomy:\n- Cerebrum\n- Cerebellum\n- Brainstem\n- Frontal, parietal, temporal, and occipital lobes\n- Ventricular system\n- Meninges\n\n" +
                       "Clinical notes:\nStroke, traumatic brain injury, tumors, seizures, neurodegeneration.";

            case OrganController.OrganType.Lung:
                return "Function:\nPerforms gas exchange between inspired air and blood.\n\n" +
                       "Key anatomy:\n- Trachea and bronchi\n- Right and left lungs\n- Lobes and fissures\n- Bronchioles and alveoli\n- Pleura\n- Pulmonary vessels\n\n" +
                       "Clinical notes:\nAsthma, pneumonia, COPD, pulmonary embolism, pleural effusion.";

            case OrganController.OrganType.Liver:
                return "Function:\nMetabolism, detoxification, bile production, glycogen storage, and protein synthesis.\n\n" +
                       "Key anatomy:\n- Right and left lobes\n- Hepatic artery\n- Portal vein\n- Hepatic veins\n- Biliary tree\n- Functional hepatic lobules\n\n" +
                       "Clinical notes:\nHepatitis, cirrhosis, fatty liver disease, portal hypertension.";

            default:
                return GetOverview();
        }
    }
}

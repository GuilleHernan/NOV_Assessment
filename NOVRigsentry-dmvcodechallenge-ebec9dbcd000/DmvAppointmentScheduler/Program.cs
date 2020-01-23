using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace DmvAppointmentScheduler
{
    class Program
    {
        public static Random random = new Random();
        public static List<Appointment> appointmentList = new List<Appointment>();
        static void Main(string[] args)
        {
            CustomerList customers = ReadCustomerData();
            TellerList tellers = ReadTellerData();
            Calculation(customers, tellers);
            OutputTotalLengthToConsole();

        }
        private static CustomerList ReadCustomerData()
        {
            string fileName = "CustomerData.json";
            string path = Path.Combine(Environment.CurrentDirectory, @"InputData\", fileName);
            string jsonString = File.ReadAllText(path);
            CustomerList customerData = JsonConvert.DeserializeObject<CustomerList>(jsonString);
            return customerData;

        }
        private static TellerList ReadTellerData()
        {
            string fileName = "TellerData.json";
            string path = Path.Combine(Environment.CurrentDirectory, @"InputData\", fileName);
            string jsonString = File.ReadAllText(path);
            TellerList tellerData = JsonConvert.DeserializeObject<TellerList>(jsonString);
            return tellerData;

        }
        static void Calculation(CustomerList customers, TellerList tellers)
        {

            // Your code goes here .....
            // Re-write this method to be more efficient instead of a assigning all customers to the same teller

            //Create a list of al the customer orderby type and the by te longest time 
            IList<Customer> customersSorted = new List<Customer>();
            customersSorted = customers.Customer.OrderBy(x => x.type).ThenByDescending(o => o.duration).ToList();
            
            foreach (Customer customer in customersSorted)
            {
                IList<Appointment> tellersDuration = new List<Appointment>();

                //Create a list of the appointment time of a customer for all tellers
                foreach (Teller teller in tellers.Teller)
                {
                                        
                    var appointment = new Appointment(customer, teller);
                    tellersDuration.Add(appointment);                  
                    
                }

                
                var sortedTellers = tellersDuration.OrderBy(o => o.duration).ToList();

                //if no appointmet exist yet, add the customer to the quickest teller
                if (appointmentList.Count == 0)
                {
                    appointmentList.Add(sortedTellers[0]);
                }
                
                else 
                {
                    
                    for (int i = 0; i < sortedTellers.Count(); i++)
                    {                        

                        bool containsItem = appointmentList.Any(o => o.teller.id == sortedTellers[i].teller.id);
                                                
                        //if the quickest teller doesn't existe yet, we add that appointment
                        if (!containsItem && appointmentList.Count() < sortedTellers.Count())
                        {
                            appointmentList.Add(sortedTellers[i]);
                            break;
                        }
                        //once i add one customer to all teller we can start adding more custemer to the same tellers
                        else if (containsItem && appointmentList.Count() >= sortedTellers.Count())
                        {

                            var tellerAppointments =
                            from appointments in appointmentList
                            group appointments by appointments.teller into tellerGroup
                            select new
                            {
                                teller = tellerGroup.Key,
                                totalDuration = tellerGroup.Sum(x => x.duration),
                            };

                            double totalTime = 0;
                            int tellerIndexAdd = 0;
                            var tellerAppointmentsSorted = tellerAppointments.OrderBy(x => x.totalDuration).ToList();
                            
                            //iterate through the teller order by the total duration of their appointments
                            for (int j = 0; j < tellerAppointmentsSorted.Count(); j++)
                            {

                                var tellerTotalDurationId = tellerAppointmentsSorted[j].teller.id;
                                int tellerIndex = sortedTellers.FindIndex(a => a.teller.id == tellerTotalDurationId);

                                //if it is the first time in this loop we update the 
                                //total time to the total time plus the duration of the new apppoitment
                                //and get that teller index to add the appointment
                                if (totalTime == 0)
                                {
                                    totalTime = tellerAppointmentsSorted[j].totalDuration + sortedTellers[tellerIndex].duration;
                                    tellerIndexAdd = tellerIndex;
                                }
                                //if the total time plus the new appointment time of the previous teller is  higher than the next teller 
                                //then we update the total time for the fastest one and get that telle index to add the new appointment
                                else if(totalTime > tellerAppointmentsSorted[j].totalDuration + sortedTellers[tellerIndex].duration)
                                {
                                    totalTime = tellerAppointmentsSorted[j].totalDuration + sortedTellers[tellerIndex].duration;
                                    tellerIndexAdd = tellerIndex;
                                }
                                
                            }

                            appointmentList.Add(sortedTellers[tellerIndexAdd]);
                            break;   

                        }
                    }
                }
            }
        }
        static void OutputTotalLengthToConsole()
        {
            var tellerAppointments =
                from appointment in appointmentList
                group appointment by appointment.teller into tellerGroup
                select new
                {
                    teller = tellerGroup.Key,
                    totalDuration = tellerGroup.Sum(x => x.duration),
                };
            var max = tellerAppointments.OrderBy(i => i.totalDuration).LastOrDefault();
            Console.WriteLine("Teller " + max.teller.id + " will work for " + max.totalDuration + " minutes!");
        }

    }
}
